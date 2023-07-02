/***
 * Author: Yunhan Li
 * Any issue please contact yunhn.lee@gmail.com
 ***/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VRUiKits.Utils
{
    public class LaserPointer : MonoBehaviour
    {
        LineRenderer lr;

        #region MonoBehaviour Callbacks
        void Start()
        {
            lr = GetComponent<LineRenderer>();
        }

        void LateUpdate()
        {
            lr.SetPosition(0, transform.position);
            RaycastHit hit;
            Vector3 fwd = transform.TransformDirection(Vector3.forward);

            if (Physics.Raycast(transform.position, fwd, out hit))
            {
                if (hit.collider)
                {
                    lr.SetPosition(1, hit.point);
                }
            }
            else
            {
                lr.SetPosition(1, transform.forward * 100);
            }
        }
        #endregion

    }
}
