#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public class TestCharacters : MonoBehaviour
{

    private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);
    // Start is called before the first frame update
    void Start()
    {
        //Character Generic = CharacterManager.instance.CreateCharacter("Generic");
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        Character_Sprite guard1 = CreateCharacter("Guard as Generic") as Character_Sprite;
        Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
        Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;

        yield return new WaitForSeconds(1);

        Raelin.SetPriority(1);
        Student.SetPriority(5);
        guard1.SetPriority(1000);

        yield return new WaitForSeconds(1);
        CharacterManager.instance.SortCharacters(new string[] {"Raelin", "Female Student 2" });

        yield return new WaitForSeconds(1);

        CharacterManager.instance.SortCharacters();

        yield return new WaitForSeconds(1);

        CharacterManager.instance.SortCharacters(new string[] {"Guard", "Female Student 2", "Raelin" });

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
#endif