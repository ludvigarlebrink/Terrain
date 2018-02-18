using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.UI
{

    public class FPSCounter : MonoBehaviour
    {
        #region Public Variables
        public int AverageFPS { get; private set; }
        public int frameRange = 60;

        public int HighestFPS { get; private set; }
        public int LowestFPS { get; private set; }
        #endregion

        #region Private Variables
        private int[] fpsBuffer;
        private int fpsBufferIndex;
        #endregion

        #region Public Functions
        public void InitializeBuffer()
        {
            if (frameRange <= 0)
            {
                frameRange = 1;
            }
            fpsBuffer = new int[frameRange];
            fpsBufferIndex = 0;
        }
        #endregion

        #region Private Functipns
        private void UpdateBuffer()
        {
            fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
            if (fpsBufferIndex >= frameRange)
            {
                fpsBufferIndex = 0;
            }
        }

        private void CalculateFPS()
        {
            int sum = 0;
            int highest = 0;
            int lowest = int.MaxValue;
            for (int i = 0; i < frameRange; i++)
            {
                int fps = fpsBuffer[i];
                sum += fps;
                if (fps > highest)
                {
                    highest = fps;
                }
                if (fps < lowest)
                {
                    lowest = fps;
                }
            }
            AverageFPS = sum / frameRange;
            HighestFPS = highest;
            LowestFPS = lowest;
        }

        private void Update()
        {
            if (fpsBuffer == null || fpsBuffer.Length != frameRange)
            {
                InitializeBuffer();
            }

            UpdateBuffer();
            CalculateFPS();
        }
        #endregion
    }

}
