using System;

namespace SgrMystiaAssistant;

[AttributeUsage(AttributeTargets.Method)]
public sealed class OnMainThreadAttribute : Attribute { }
