using UnityEngine;
using UnityEngine.UI;

namespace Com.HorusGames.Runtime.Avatar
{
    public abstract class AvatarProfile : MonoBehaviour
    {
        [SerializeField] protected Image icon;
        [SerializeField] protected Image frame;

        public Sprite IconSprite => icon != null ? icon.sprite : null;
        public Sprite FrameSprite => frame != null ? frame.sprite : null;

        public virtual void SetAvatar(Sprite iconSprite, Sprite frameSprite)
        {
            if (icon != null) icon.sprite = iconSprite;
            if (frame != null) frame.sprite = frameSprite;
        }
    }
}