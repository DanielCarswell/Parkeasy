using Microsoft.AspNetCore.Session;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

public static class SessionExtensions
{
    //How to use
    //HttpContext.Session.SetObjectAsJson("Test", myComplexObject); Set object.
    // var myComplexObject = HttpContext.Session.GetObjectFromJson<MyClass>("Test"); Retrieve object.
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);

        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
}