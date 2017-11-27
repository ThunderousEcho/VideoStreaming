function evaluateNonStrict(code: String) {

#if UNITY_IOS

    return "JS eval not supported on iOS. Use C# instead."

#else

    return eval(code);

#endif

}