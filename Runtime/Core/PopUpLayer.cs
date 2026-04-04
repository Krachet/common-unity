using UnityEngine;

public abstract class PopUpLayer : MonoBehaviour
{
    [SerializeField] protected GameObject context;
    public bool IsActive => context.activeSelf;
    public UILayer AttachedUILayer { get; set; }

    public virtual void Show()
    {
        context?.SetActive(true);
    }

    public virtual void Hide()
    {
        context?.SetActive(false);
    }

    protected virtual void Awake()
    {
        GameUI.RegisterPopUp(this);
        context?.SetActive(false);
    }
}