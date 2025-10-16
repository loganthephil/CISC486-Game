// RequiredFieldAttribute by adammyhre
// From: https://gist.github.com/adammyhre/a7c14b094ff2bdfb0a86df0579b4c539
// Displays information in the inspector if a field is not assigned a value

using System;
using UnityEngine;

namespace DroneStrikers.Core.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredFieldAttribute : PropertyAttribute { }
}