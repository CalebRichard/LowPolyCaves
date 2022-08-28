using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class Functions 
{
    /// <summary>
    /// Returns time oscillating value between -1 and 1
    /// </summary>
    /// <param name="t">Time</param>
    /// <returns></returns>
    public static float Oscillator(float t) {
        
        return Sin(PI * t);
    }
    /// <summary>
    /// Returns position and time oscillating value between -1 and 1
    /// </summary>
    /// <param name="x">Position</param>
    /// <param name="t">Time</param>
    /// <returns></returns>
    public static float Oscillator(float x, float t) {

        return Oscillator(x + t);
    }

    /// <summary>
    /// Returns time oscillating value between specified range
    /// </summary>
    /// <param name="t">Time</param>
    /// <param name="min">Range Minimum</param>
    /// <param name="max">Range Maximum</param>
    /// <returns></returns>
    public static float Oscillator(float t, float min, float max) {

        return 0.5f * (Oscillator(t) + 1) * (max - min) + min;
    }

    /// <summary>
    /// Returns position and time oscillating value between specified range
    /// </summary>
    /// <param name="x">Position</param>
    /// <param name="t">Time</param>
    /// <param name="min">Range Minimum</param>
    /// <param name="max">Range Maximum</param>
    /// <returns></returns>
    public static float Oscillator(float x, float t, float min, float max) {

        return 0.5f * (Oscillator(x, t) + 1) * (max - min) + min;
    }

    /// <summary>
    /// Returns random value between specified range
    /// </summary>
    /// <param name="min">Range Minimum</param>
    /// <param name="max">Range Maximum</param>
    /// <returns></returns>
    public static float RandomScaled(float min, float max) {

        return 0.5f * (Random.value + 1) * (max - min) + min;
    }

    // Scales float value from one scale to another
    public static float ScaleToRange(float v, float min, float max, float smin, float smax) {

        return smin + (v - min) / (max - min) * (smax - smin);
    }
}
