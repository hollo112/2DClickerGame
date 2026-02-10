#if UNITY_WEBGL && !UNITY_EDITOR
using System;

namespace Firebase.Firestore
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class FirestoreDataAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class FirestorePropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class FirestoreDocumentIdAttribute : Attribute { }
}
#endif
