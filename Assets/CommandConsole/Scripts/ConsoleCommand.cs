using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharlesOlinerCommandConsole {
    public class ConsoleCommand : MonoBehaviour {
        protected static GameObject so {
            get {
                return CommandConsole.main.listener.selectedObject;
            }
            set {
                CommandConsole.main.listener.selectedObject = value;
            }
        }

        //wrapper class for commands to give them more functions.
        //you can add more functions here if you want, but they will only show up in the auto-type list if you call CommandConsole.AddHotkey().
        //(they also won't be usable if they're private and will be very difficult to use if they're not static.)
        public static void members(Type t) {members(t, null);}
        public static void members(Type t, string search = null) {
            HashSet<string> autocompleteWords = new HashSet<string>();
            CommandConsole.TypeEnumerate(ref autocompleteWords, new Type[] {t});

            if (search == null) {
                foreach (string s in autocompleteWords) {
                    Debug.Log(s);
                }
            } else {
                foreach (string s in autocompleteWords) {
                    if (s.ToLower().StartsWith(search.ToLower()))
                        Debug.Log(s);
                }
            }
        }

        public static GameObject summon(string resourceName) {
            if (resourceName == null) {
                Debug.LogError("summon(): Resource name cannot be null.");
                return null;
            }
            object o = Resources.Load(resourceName);
            if (o == null) {
                Debug.LogError("summon(): Could not find " + resourceName + " in resources folder.");
                return null;
            }
            if (o.GetType() != typeof(GameObject)) {
                Debug.LogError("summon(): " + resourceName + " is not a GameObject.");
                return null;
            }
            RaycastHit h;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out h)) {
                return Instantiate((GameObject)o, h.point, Camera.main.transform.rotation);
            } else {
                Debug.LogWarning("summon(): Mouse pointer is not over a location to summon at. Summoning at main camera.");
                return Instantiate((GameObject)o, Camera.main.transform.position, Camera.main.transform.rotation);
            }
        }

        public static void relocate(GameObject relocatee) {
            if (relocatee == null) {
                Debug.LogError("relocate(): Target is null.");
                return;
            }
            Rigidbody r = relocatee.GetComponent<Rigidbody>();
            if (r != null) {
                r.AddForce(-r.velocity, ForceMode.VelocityChange);
                r.AddTorque(-r.angularVelocity, ForceMode.VelocityChange);
            }
            RaycastHit h;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out h)) {
                relocatee.transform.position = h.point + h.normal * relocatee.transform.localScale.y;
            } else {
                Debug.LogError("relocate(): Mouse pointer is not over a location to teleport to.");
            }
        }

        public static void push(GameObject pushee) {
            push(pushee, Camera.main.transform.forward * 5);
        }
        public static void push(GameObject pushee, float pushSpeed) {
            push(pushee, Camera.main.transform.forward * pushSpeed);
        }
        public static void push(GameObject pushee, Vector3 pushVec) {
            if (pushee == null) {
                Debug.LogError("push(): Target is null.");
                return;
            }
            Rigidbody r = pushee.GetComponent<Rigidbody>();
            if (r == null) {
                Debug.LogError("push(): Target does not have a rigidbody.");
                return;
            }
            r.AddForce(pushVec, ForceMode.VelocityChange);
        }
    }
}
