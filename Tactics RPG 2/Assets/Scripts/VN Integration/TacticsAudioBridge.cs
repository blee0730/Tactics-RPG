using UnityEngine;

// Replaces MusicPlayer, AudioTracker, and AudioSequence entirely.
// Add this to a persistent GameObject (or BattleController) instead.
// AudioManager (VN system) handles all actual playback via mixer groups.
//
// Channel layout:
//   Channel 0 — Battle music (looping, cross-fades on track change)
//   Channel 1 — Cutscene/VN music (pauses battle music while active)
//   Channel 2 — Ambient / environment loops
//
// SFX and Voice: use AudioManager.instance.PlaySoundEffect() / .PlayVoice() directly.

public class TacticsAudioBridge : MonoBehaviour
{
    public const int CHANNEL_BATTLE   = 0;
    public const int CHANNEL_CUTSCENE = 1;
    public const int CHANNEL_AMBIENT  = 2;

    [Header("Startup Music")]
    [Tooltip("Path under Resources/ to the battle music clip (no extension)")]
    public string defaultBattleMusic = "Audio/Music/Battle_Theme";

    void Start()
    {
        if (!string.IsNullOrEmpty(defaultBattleMusic))
            PlayBattleMusic(defaultBattleMusic);
    }

    // ── Battle music ──────────────────────────────────────────────────────────

    // Plays a looping battle track. Cross-fades from whatever was playing.
    public void PlayBattleMusic(string resourcePath, float volumeCap = 0.8f)
    {
        AudioManager.instance.PlayTrack(
            resourcePath,
            channel:        CHANNEL_BATTLE,
            loop:           true,
            startingVolume: 0f,
            volumeCap:      volumeCap
        );
    }

    // Plays an intro clip then seamlessly transitions to a looping clip.
    // Replicates the old MusicPlayer.Start() intro+loop behaviour.
    public void PlayBattleMusicWithIntro(string introPath, string loopPath, float volumeCap = 0.8f)
    {
        StartCoroutine(IntroThenLoop(introPath, loopPath, volumeCap));
    }

    System.Collections.IEnumerator IntroThenLoop(string introPath, string loopPath, float volumeCap)
    {
        UnityEngine.AudioClip intro = Resources.Load<UnityEngine.AudioClip>(introPath);
        if (intro == null)
        {
            Debug.LogWarning($"[TacticsAudioBridge] Intro clip not found: {introPath}");
            PlayBattleMusic(loopPath, volumeCap);
            yield break;
        }

        AudioManager.instance.PlayTrack(introPath, CHANNEL_BATTLE, loop: false,
            startingVolume: 0f, volumeCap: volumeCap);

        yield return new WaitForSeconds(intro.length);

        PlayBattleMusic(loopPath, volumeCap);
    }

    public void StopBattleMusic(bool immediate = false)
    {
        AudioManager.instance.StopTrack(CHANNEL_BATTLE);
    }

    // ── Cutscene music ────────────────────────────────────────────────────────

    // Called by TacticsCutSceneBridge at cutscene start.
    // Ducks battle music and plays VN music.
    public void StartCutSceneMusic(string resourcePath, float volumeCap = 0.8f)
    {
        // Lower battle music during cutscene
        var battleChannel = AudioManager.instance.TryGetChannel(CHANNEL_BATTLE);
        if (battleChannel?.activeTrack != null)
            battleChannel.activeTrack.volumeCap = 0.15f; // duck to 15%

        AudioManager.instance.PlayTrack(
            resourcePath,
            channel:        CHANNEL_CUTSCENE,
            loop:           true,
            startingVolume: 0f,
            volumeCap:      volumeCap
        );
    }

    // Called by TacticsCutSceneBridge when cutscene ends.
    public void EndCutSceneMusic()
    {
        AudioManager.instance.StopTrack(CHANNEL_CUTSCENE);

        // Restore battle music volume
        var battleChannel = AudioManager.instance.TryGetChannel(CHANNEL_BATTLE);
        if (battleChannel?.activeTrack != null)
            battleChannel.activeTrack.volumeCap = 0.8f;
    }

    // ── SFX convenience wrappers ──────────────────────────────────────────────

    public void PlaySFX(string resourcePath, float volume = 1f, float pitch = 1f)
    {
        AudioManager.instance.PlaySoundEffect(resourcePath, volume: volume, pitch: pitch);
    }

    public void PlayVoice(string resourcePath, float volume = 1f)
    {
        AudioManager.instance.PlayVoice(resourcePath, volume: volume);
    }

    // ── Volume control (hook these to your settings UI sliders) ──────────────

    public void SetMusicVolume(float normalised, bool muted = false)
        => AudioManager.instance.SetMusicVolume(normalised, muted);

    public void SetSFXVolume(float normalised, bool muted = false)
        => AudioManager.instance.SetSFXVolume(normalised, muted);

    public void SetVoiceVolume(float normalised, bool muted = false)
        => AudioManager.instance.SetVoicesVolume(normalised, muted);
}
