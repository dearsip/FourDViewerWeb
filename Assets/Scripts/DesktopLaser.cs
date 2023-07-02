using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VRUiKits.Utils
{
    public class DesktopLaser : MonoBehaviour
    {
        public Camera c;
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
            if (Input.GetMouseButton(0) && Physics.Raycast(c.ScreenPointToRay(Input.mousePosition), out hit))
            {
                lr.enabled = true;
                if (hit.collider)
                {
                    lr.SetPosition(1, hit.point);
                }
            }
            else
            {
                lr.enabled = false;
            }
        }
        #endregion

    }
}
