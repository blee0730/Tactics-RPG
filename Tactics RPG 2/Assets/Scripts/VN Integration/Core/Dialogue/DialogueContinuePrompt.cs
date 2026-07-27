using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueContinuePrompt : MonoBehaviour
{
    private RectTransform root;
    private RectTransform arrowRect;

    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI tmpro;

    [Header("Positioning")]
    [SerializeField] private Vector2 endOfTextOffset = new Vector2(18f, 0f);

    public bool isShowing => anim != null && anim.gameObject.activeSelf;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if(root == null)
            root = GetComponent<RectTransform>();

        if(arrowRect == null && anim != null)
            arrowRect = anim.GetComponent<RectTransform>();
    }

    public void Show()
    {
        Initialize();

        if(tmpro == null || string.IsNullOrEmpty(tmpro.text))
        {
            if(isShowing)
                Hide();

            return;
        }

        tmpro.ForceMeshUpdate(true);

        TMP_TextInfo textInfo = tmpro.textInfo;
        if(textInfo == null || textInfo.characterCount == 0)
        {
            if(isShowing)
                Hide();

            return;
        }

        int finalCharacterIndex = GetLastVisibleCharacterIndex(textInfo);
        if(finalCharacterIndex < 0)
        {
            if(isShowing)
                Hide();

            return;
        }

        TMP_CharacterInfo finalCharacter = textInfo.characterInfo[finalCharacterIndex];
        Vector3 targetPos = finalCharacter.bottomRight;
        targetPos += new Vector3(endOfTextOffset.x, endOfTextOffset.y, 0f);

        anim.gameObject.SetActive(true);

        root.SetParent(tmpro.rectTransform, false);
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;
        root.localPosition = targetPos;

        // The arrow child can keep stale scene offsets if it was positioned while
        // the prompt lived under the canvas.  The root follows the text end; the
        // arrow child should stay centered inside that root.
        if(arrowRect != null)
        {
            arrowRect.localRotation = Quaternion.identity;
            arrowRect.localScale = Vector3.one;
            arrowRect.anchoredPosition = Vector2.zero;
        }
    }

    private int GetLastVisibleCharacterIndex(TMP_TextInfo textInfo)
    {
        for(int i = textInfo.characterCount - 1; i >= 0; --i)
        {
            if(textInfo.characterInfo[i].isVisible)
                return i;
        }

        return -1;
    }

    public void Hide()
    {
        if(anim != null)
            anim.gameObject.SetActive(false);
    }
}
