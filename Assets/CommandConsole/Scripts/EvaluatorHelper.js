#pragma strict

@HideInInspector
public var evaluator: Evaluator;

static var selectedObject: GameObject;

function evaluate(code: String) {
    var result = evaluator.evaluateNonStrict(code);

    if (result != null) {
        gameObject.SendMessage("ProcessCommandMiddleUnityScript", result);
    }
    else {
        gameObject.SendMessage("ProcessCommandMiddleUnityScriptNull", result);
    }

    if (selectedObject != null) {
        gameObject.SendMessage("setSelectedObject", selectedObject);
    } else {
        gameObject.SendMessage("setSelectedObjectNull");
    }
}

function setSelected(so: GameObject) {
    selectedObject = so;
}

function setSelectedNull() {
    selectedObject = null;
}
