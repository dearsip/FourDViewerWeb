//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// namespace Valve.VR.InteractionSystem
// {
    //-------------------------------------------------------------------------
    // [RequireComponent(typeof(Interactable))]
    public class SliderDrive : MonoBehaviour // : LinearDrive
    {
        public Slider slider;
        float min, max;

        // protected override void Start()
        // {
            // if (linearMapping == null)
            // {
                // linearMapping = GetComponent<LinearMapping>();
            // }

            // if (linearMapping == null)
            // {
                // linearMapping = gameObject.AddComponent<LinearMapping>();
            // }

            // min = slider.minValue;
            // max = slider.maxValue;
            // linearMapping.value = 0; //Mathf.InverseLerp(min, max, slider.value);
            // initialMappingOffset = linearMapping.value;
            // //transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
        // }

        // protected override void HandHoverUpdate( Hand hand )
        // {
            // GrabTypes startingGrabType = hand.GetGrabStarting();

            // if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            // {
                // //initialMappingOffset = linearMapping.value - CalculateLinearMapping( hand.transform );
				// sampleCount = 0;
				// mappingChangeRate = 0.0f;

                // hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            // }
		// }

        // protected override void OnDetachedFromHand(Hand hand)
        // {
        // }

        // protected override float CalculateLinearMapping(Transform updateTransform)
        // {
            // Vector3 direction = endPosition.position - startPosition.position;
            // float length = direction.magnitude;
            // direction.Normalize();

            // RaycastHit hit;
            // Physics.Raycast(updateTransform.position, updateTransform.forward, out hit, 100f);
            // Vector3 position = (hit.collider) ? hit.point : updateTransform.position;
            // Vector3 displacement = position - startPosition.position;

            // return Vector3.Dot(displacement, direction) / length;
        // }

        // protected override void UpdateLinearMapping(Transform updateTransform)
        // {
            // prevMapping = linearMapping.value;
            // linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

            // mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = (1.0f / Time.deltaTime) * (linearMapping.value - prevMapping);
            // sampleCount++;

            // slider.value = Mathf.Lerp(min, max, linearMapping.value);
            // linearMapping.value = Mathf.InverseLerp(min, max, slider.value);

            // //if (repositionGameObject)
            // //{
                // //transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
            // //}

        // }

		// protected override void Update()
        // {
            // if ( maintainMomemntum && mappingChangeRate != 0.0f )
			// {
				// //Dampen the mapping change rate and apply it to the mapping
				// mappingChangeRate = Mathf.Lerp( mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime );
				// linearMapping.value = Mathf.Clamp01( linearMapping.value + ( mappingChangeRate * Time.deltaTime ) );

				// //if ( repositionGameObject )
				// //{
					// //transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
				// //}
			// }
		// }

        // public void PassiveUpdate() {
            // linearMapping.value = Mathf.InverseLerp(min, max, slider.value);
            // initialMappingOffset = linearMapping.value;
            // //transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
        // }
    }
// }