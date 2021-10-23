using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace AquaEffects.AquaPostOutline
{
    [ExecuteInEditMode, VolumeComponentMenu("AquaEffects/AquaPostOutline")]
    public class AquaPostOutline : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter enable = new BoolParameter(false);
        [Tooltip("Number of pixels between samples that are tested for an edge. When this value is 1, tested samples are adjacent.")]
        public FloatParameter scale = new FloatParameter(1.0f);
        public ColorParameter color = new ColorParameter(Color.black);
        [Tooltip("Difference between depth values, scaled by the current depth, required to draw an edge.")]
        public FloatParameter depthThreshold = new FloatParameter(1.5f);
        [Range(0, 1), Tooltip("The value at which the dot product between the surface normal and the view direction will affect " +
            "the depth threshold. This ensures that surfaces at right angles to the camera require a larger depth threshold to draw " +
            "an edge, avoiding edges being drawn along slopes.")]
        public FloatParameter depthNormalThreshold = new FloatParameter(0.5f);
        [Tooltip("Scale the strength of how much the depthNormalThreshold affects the depth threshold.")]
        public FloatParameter depthNormalThresholdScale = new FloatParameter(7);
        [Range(0, 1), Tooltip("Larger values will require the difference between normals to be greater to draw an edge.")]
        public FloatParameter normalThreshold = new FloatParameter(0.4f);

        public bool IsActive() => active && enable.value;

        public bool IsTileCompatible() => false;
    }
}