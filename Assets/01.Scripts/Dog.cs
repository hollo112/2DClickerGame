#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using Firebase.Firestore;
using UnityEngine;

[Serializable]
[FirestoreData]
public class Dog
{
    [FirestoreDocumentId]
    public string Id {get; set;}        // 문서의 고유 식별자가 자동으로 맵핑된다
    
    [FirestoreProperty]
    public string Name {get; set;}      // 필드가 아니라 get/set이 있는 프로퍼티여야한다.
    
    [FirestoreProperty]
    public int Age {get; set;}

    public Dog() { }
    
    public Dog(string name, int age)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new System.ArgumentNullException("이름은 비어있을 수 없습니다.");
        }

        if (age <= 0)
        {
            throw new System.ArgumentNullException("나이는  0살보다 작을 수 없습니다.");
        }
        
        Name = name;
        Age = age;
    }
}
#endif
