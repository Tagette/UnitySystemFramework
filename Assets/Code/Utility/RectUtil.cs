using System;
using UnityEngine;

namespace UnitySystemFramework.Utility
{
    public static class RectUtil
    {
        public static void SplitX(this Rect rect, out Rect left, out Rect right)
        {
            var half = rect.width / 2;
            left = new Rect(rect.x, rect.y, half, rect.height);
            right = new Rect(rect.x + half, rect.y, half, rect.height);
        }

        public static void SplitX(this Rect rect, out Rect left, out Rect mid, out Rect right)
        {
            var third = rect.width / 3;
            left =  new Rect(rect.x, rect.y, third, rect.height);
            mid =   new Rect(rect.x + third, rect.y, third, rect.height);
            right = new Rect(rect.x + third + third, rect.y, third, rect.height);
        }

        public static void SplitX(this Rect rect, out Rect left, out Rect midLeft, out Rect midRight, out Rect right)
        {
            var fourth = rect.width / 4;
            left = new Rect(rect.x, rect.y, fourth, rect.height);
            midLeft = new Rect(rect.x + fourth, rect.y, fourth, rect.height);
            midRight = new Rect(rect.x + fourth + fourth, rect.y, fourth, rect.height);
            right = new Rect(rect.x + fourth + fourth + fourth, rect.y, fourth, rect.height);
        }

        public static void SplitY(this Rect rect, out Rect top, out Rect bottom)
        {
            var half = rect.height / 2;
            top = new Rect(rect.x, rect.y, rect.width, half);
            bottom = new Rect(rect.x, rect.y + half, rect.width, half);
        }

        public static void SplitY(this Rect rect, out Rect top, out Rect mid, out Rect bottom)
        {
            var third = rect.height / 3;
            top = new Rect(rect.x, rect.y, rect.width, third);
            mid = new Rect(rect.x, rect.y + third, rect.width, third);
            bottom = new Rect(rect.x, rect.y + third + third, rect.width, third);
        }

        public static void SplitY(this Rect rect, out Rect top, out Rect midTop, out Rect midBottom, out Rect bottom)
        {
            var fourth = rect.height / 4;
            top = new Rect(rect.x, rect.y, rect.width, fourth);
            midTop = new Rect(rect.x, rect.y + fourth, rect.width, fourth);
            midBottom = new Rect(rect.x, rect.y + fourth + fourth, rect.width, fourth);
            bottom = new Rect(rect.x, rect.y + fourth + fourth + fourth, rect.width, fourth);
        }

        public static Rect[] SplitX(this Rect rect, int num)
        {
            var res = new Rect[num];
            var dividend = rect.width / num;
            for (int i = 0; i < num; i++)
            {
                res[i] = new Rect(rect.x + (dividend * i), rect.y, dividend, rect.height);
            }

            return res;
        }

        public static Rect[] SplitY(this Rect rect, int num)
        {
            var res = new Rect[num];
            var dividend = rect.height / num;
            for (int i = 0; i < num; i++)
            {
                res[i] = new Rect(rect.x, rect.y + (dividend * i), rect.width, dividend);
            }

            return res;
        }

        public static Rect SplitQ(this Rect rect, int quad)
        {
            var halfW = rect.width / 2;
            var halfH = rect.height / 2;
            switch (quad)
            {
                case 0:
                    return new Rect(rect.x, rect.y, halfW, halfH);
                case 1:
                    return new Rect(rect.x + halfW, rect.y, halfW, halfH);
                case 2:
                    return new Rect(rect.x + halfW, rect.y + halfH, halfW, halfH);
                case 3:
                    return new Rect(rect.x, rect.y + halfH, halfW, halfH);
                default:
                    throw new Exception("Invalid quad number. (0-3)");
            }
        }
    }
}
