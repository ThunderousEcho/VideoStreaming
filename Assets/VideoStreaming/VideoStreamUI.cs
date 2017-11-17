using UnityEngine;
using UnityEngine.UI;

public class VideoStreamUI : MonoBehaviour {

    /// <summary>The component responsible for sending video information to ffmpeg.</summary>
    public VideoStreamSource source;

    /// <summary>The InputField into which the user enters a stream key.</summary>
    public InputField keyField;

    /// <summary>The 'Submit' button pressed once the stream key is entered into the input field.</summary>
    public Button submitButton;

    /// <summary>The InputField into which the user enters a video id.</summary>
    public InputField idField;

    /// <summary>The 'Submit' button pressed once the video id is entered into the input field.</summary>
    public Button idSubmitButton;

    /// <summary>The color to be displayed on the input field when the stream key entered by the user is incorrectly formatted.</summary>
    public Color keyIncorrectlyFormattedColor;

    /// <summary>The color to be displayed on the input field when the stream key entered by the user is correctly formatted.</summary>
    public Color keyCorrectlyFormattedColor;

    /// <summary>
    /// Recalls the remembered streaming key.
    /// </summary>
    void Start() {
        if (PlayerPrefs.HasKey("Stream Key"))
            keyField.text = PlayerPrefs.GetString("Stream Key");
        OnKeyValueChanged();
    }

    /// <summary>
    /// Called when the stream key entered into the input field changes.
    /// </summary>
    public void OnKeyValueChanged() {
        string key = keyField.text;
        var formattingCorrectness = IsStreamKeyFormattedCorrectly(key);

        //set the color of the inputfield tp reflect how correctly the key entered by the user is formatted.
        Color fieldColor = Color.white;
        switch (formattingCorrectness) {
            case FormattingCorrectness.correct:
                fieldColor = keyCorrectlyFormattedColor;
                break;
            case FormattingCorrectness.incorrect:
                fieldColor = keyIncorrectlyFormattedColor;
                break;
        }
        keyField.image.color = fieldColor;

        //disable the submit button when the entered key is not valid.
        submitButton.interactable = formattingCorrectness == FormattingCorrectness.correct;
    }

    /// <summary>
    /// Called when the Submit button is pressed. Starts the stream, if the key is correctly formatted.
    /// </summary>
    public void OnSubmit() {
        string key = keyField.text;
        if (IsStreamKeyFormattedCorrectly(key) == FormattingCorrectness.correct) {
            source.key = key;
            source.enabled = true; //enable the video streaming component
            gameObject.SetActive(false); //close this UI
            PlayerPrefs.SetString("Stream Key", key); //remember the streaming key
        }
    }

    /// <summary>
    /// Called when the video id entered into the input field changes.
    /// </summary>
    public void OnIdValueChanged() {
        string id = idField.text;
        var formattingCorrectness = IsStreamIdFormattedCorrectly(id);

        //set the color of the inputfield tp reflect how correctly the key entered by the user is formatted.
        Color fieldColor = Color.white;
        switch (formattingCorrectness) {
            case FormattingCorrectness.correct:
                fieldColor = keyCorrectlyFormattedColor;
                break;
            case FormattingCorrectness.incorrect:
                fieldColor = keyIncorrectlyFormattedColor;
                break;
        }
        idField.image.color = fieldColor;

        //disable the submit button when the entered key is not valid.
        idSubmitButton.interactable = formattingCorrectness == FormattingCorrectness.correct;
    }

    /// <summary>
    /// Called when the Submit button is pressed. Opens a web browser, if the id is correctly formatted.
    /// </summary>
    public void OnSubmitId() {
        if (IsStreamIdFormattedCorrectly(idField.text) == FormattingCorrectness.correct)
            VideoStreamWebBrowserOpener.Open(idField.text);
    }

    /// <summary>Indicates how correctly a given stream key is formatted.</summary>
    public enum FormattingCorrectness {
        /// <summary>The key is formatted correctly, and is complete and ready to use.</summary>
        correct = 0,
        /// <summary>The key is formatted incorrectly, and must be corrected before use.</summary>
        incorrect = 1,
        /// <summary>The key is formatted correctly so far, but is not yet complete and cannot be used.</summary>
        incomplete = 2
    }

    /// <summary>
    /// Checks whether the given string is a properly formatted YouTube Live stream key.
    /// </summary>
    /// <param name="key">The string to be checked.</param>
    /// <returns>Whether the stream key is correct, incorrect, or incomplete.</returns>
	static FormattingCorrectness IsStreamKeyFormattedCorrectly(string key) {

        if (key.Length > 4 && key[4] != '-')
            return FormattingCorrectness.incorrect;
        if (key.Length > 9 && key[9] != '-')
            return FormattingCorrectness.incorrect;
        if (key.Length > 14 && key[14] != '-')
            return FormattingCorrectness.incorrect;

        foreach (char c in key) {
            if (c == '-')
                continue;
            if (char.IsNumber(c))
                continue;
            if (!char.IsLetter(c))
                return FormattingCorrectness.incorrect;
            if (!char.IsLower(c))
                return FormattingCorrectness.incorrect;
        }

        return key.Length == 19 ? FormattingCorrectness.correct : FormattingCorrectness.incomplete;
    }

    /// <summary>
    /// Checks whether the given string is a properly formatted YouTube video/stream id.
    /// </summary>
    /// <param name="id">The id to be checked.</param>
    /// <returns>Whether the id is correct, incorrect, or incomplete.</returns>
    static FormattingCorrectness IsStreamIdFormattedCorrectly(string id) {

        foreach (char c in id) {
            if (c == '-' || c == '_' || char.IsLetterOrDigit(c))
                continue;
            return FormattingCorrectness.incorrect;
        }

        return id.Length == 11 ? FormattingCorrectness.correct : FormattingCorrectness.incomplete;
    }
}
