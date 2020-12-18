# Run Debug

Debug.Assert is annotated with [ConditionalAttribute("DEBUG")]. This means that
all invocations are removed by the compiler unless the DEBUG preprocessor 
symbol is defined.

1. Environment Variables : DEBUG

Mono does not show a dialog box like Microsoft's .NET implementation when an
assertion is hit. You need to set a TraceListener, e.g.
 
2. Environment Variables : MONO_TRACE_LISTENER=Console.Error

###### Reference: https://stackoverflow.com/a/7479746
