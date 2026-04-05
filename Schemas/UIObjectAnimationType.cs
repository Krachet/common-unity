using UnityEngine;

namespace Com.Krackhet.Schemas
{
    public class UIObjectAnimationType
    {
        public enum AnimationIn
        {
            None,
            FadeIn,
            ScaleIn,
            FloatIn,
        }

        public enum AnimationOut
        {
            None,
            FadeOut,
            ScaleOut,
            FloatOut,
        }
    }
}