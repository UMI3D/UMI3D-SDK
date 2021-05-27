using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    [RequireComponent(typeof(Collider))]
    public class Point : AbstractMovableObject
    {
        public MeshRenderer rnd;

        public string shaderColorPropertyName = "_Albedo";
        private float originalSaturation;
        private float originalValue;
        private float originalAlpha;
        private Color originalColor;

        public override void DisableHighlight()
        {
            rnd.material.SetColor(shaderColorPropertyName, originalColor);
        }

        public override void EnableHighlight()
        {
            originalColor = rnd.material.GetColor(shaderColorPropertyName);
            originalAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out float H, out originalSaturation, out originalValue);

            Color newColor = Color.HSVToRGB(H, Mathf.Lerp(originalSaturation, 1f, .5f), Mathf.Lerp(originalValue, 0f, .5f));
            newColor.a = originalAlpha;

            rnd.material.SetColor(shaderColorPropertyName, newColor);
        }

        public override void Move(Vector3 translation)
        {
            this.transform.Translate(translation, Space.World);
            base.Move(translation);
        }

        public void Display()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
    }
}