using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoStreamUI : MonoBehaviour {

    public VideoStreamSource source;
    public InputField keyField;
    public Button submitButton;

    public Color keyIncorrectlyFormattedColor;
    public Color keyCorrectlyFormattedColor;

    void Start() {
        OnKeyValueChanged();
    }

    public void OnKeyValueChanged() {
        string key = keyField.text;
        var formattingCorrectness = IsStreamKeyFormattedCorrectly(key);

        Color fieldColor = Color.white;
        switch (formattingCorrectness) {
            case FormattingCorrectness.correct:
                fieldColor = keyCorrectlyFormattedColor;
                break;
            case FormattingCorrectness.incorrect:
                fieldColor = keyIncorrectlyFormattedColor;
                break;
        }

        submitButton.interactable = formattingCorrectness == FormattingCorrectness.correct;

        keyField.image.color = fieldColor;
    }

    public void OnSubmit() {
        string key = keyField.text;
        if (IsStreamKeyFormattedCorrectly(key) == FormattingCorrectness.correct) {
            source.key = key;
            source.enabled = true;
            gameObject.SetActive(false);
        }
    }

    public enum FormattingCorrectness {
        correct = 0,
        incorrect = 1,
        incomplete = 2
    }

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
}
