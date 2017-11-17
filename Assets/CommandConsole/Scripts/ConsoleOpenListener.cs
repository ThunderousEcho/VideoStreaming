using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CharlesOlinerCommandConsole {
    public class ConsoleOpenListener : MonoBehaviour {

        //Since the console is disabled when it is closed, it can't tell when a user presses the open key.
        //This script enables the console whenever the open key is pressed.
        //(Should be placed on a parent of the console elements.)

        public KeyCode consleOpenKey = KeyCode.BackQuote; //the key that opens the console. Tilde (~) by default.
       
        public bool fullAutocompleteLibrary; //when checked, the library is loaded with every single thing you could possibly want to type, but most of it is useless and gets in the way of the stuff you actually want. the library only updates when unloaded and reloaded.
        public bool allowAutocomplete = true; //autocomplete takes about 4 MB if in full mode. do not change the value once Start() is called.
        public bool unloadAutocompleteLibraryWhileClosed; //will result in somewhat odd behaviour if it is changed while the console is closed.
        public GameObject selectedObject; //selected object
        public string compilerVersion = "3.5";

        [HideInInspector] public CommandConsole console;
        [HideInInspector] public GameObject consoleUIParent;
        [HideInInspector] public Text[] textsToClear; //the script also has to clear the tooltips because disabled objects don't trigger PointerExit.

        void Start() {
            if (!unloadAutocompleteLibraryWhileClosed) { //console isn't enabled to do this for itself
                unloadAutocompleteLibraryWhileClosed = true;
                console.OnEnable();
                unloadAutocompleteLibraryWhileClosed = false;
            }
        }

        void Update() {
            if (Input.GetKeyDown(consleOpenKey)) {
                consoleUIParent.SetActive(true);
                foreach (Text t in textsToClear) {
                    t.text = "";
                }
            }
        }

        /// <summary>
        /// Opens or closes the command console.
        /// </summary>
        /// <param name="open">Whether to open or close the console.</param>
        public static void SetConsoleOpen(bool open = true) {
            CommandConsole.main.listener.consoleUIParent.SetActive(open);
        }
    }
}