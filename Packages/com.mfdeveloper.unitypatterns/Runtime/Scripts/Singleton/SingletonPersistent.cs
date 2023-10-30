using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityPatterns.Singleton.Attributes;

namespace UnityPatterns.Singleton
{
    /// <summary>
    /// Generic Singleton MonoBehaviour persistent among scenes
    /// </summary>
    /// <remarks>
    /// <b>Based in:</b><br/>
    /// <a href="https://gist.github.com/mstevenson/4325117">Generic Singleton classes for Unity</a><br/>
    /// <a href="https://www.youtube.com/watch?v=Ova7l0UB26U">Design Pattern: Singletons in Unity</a>
    /// </remarks>
    /// <typeparam name="T">Class to use as a singleton</typeparam>
    public class SingletonPersistent<T> : Singleton<T> where T : Component
    {
        /// <summary>
        /// The default <see cref="BindingFlags"/> to access fields informations
        /// </summary>
        protected const BindingFlags FIELDS_FLAGS = BindingFlags.Instance 
                                                    | BindingFlags.Public
                                                    | BindingFlags.NonPublic;

        /// <summary>
        /// Get only to access the <see cref="SingletonSettingsAttribute"/> 
        /// configurations data
        /// </summary>
        public SingletonSettingsAttribute Settings => (SingletonSettingsAttribute) Attribute.GetCustomAttribute(
            GetType(), 
            typeof(SingletonSettingsAttribute)
        );

        protected override void Awake()
        {
            if (_instance != null && _instance != this)
            {

                if (Settings?.DestroyGameObject == PersistentDestroyOrder.NEXT)
                {
                    if (Settings?.CopyFieldsValues == true)
                    {
                        CopyFieldValues(source: GetComponent<T>(), target: _instance, f =>
                        {
                            return !f.IsNotSerialized && f.FieldType.IsAssignableFrom(typeof(GameObject));
                        });
                    }

                    Destroy(gameObject);
                    Destroy(this);
                }
                else
                {
                    if (Settings?.CopyFieldsValues == true)
                    {
                        CopyFieldValues(source: _instance, target: GetComponent<T>(), f =>
                        {
                            return !f.IsNotSerialized && !f.FieldType.IsAssignableFrom(typeof(GameObject));
                        });
                    }

                    Destroy(_instance.gameObject);
                    Destroy(_instance);
                }

                _instance = null;
            }

            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Get the fields name => value pairs as a <see cref="Dictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="instance">A <see cref="T"/> instance to fetch name => value pairs</param>
        /// <param name="condition">Custom <see cref="Predicate{T}"/> delegate to fetch <paramref name="instance"/> fields pairs</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of name => value of fields from <paramref name="instance"/> </returns>
        public virtual Dictionary<string, object> GetFieldsData(T instance, Predicate<FieldInfo> condition = null)
        {
            Type type = instance.GetType();
            Dictionary<string, object> values = new Dictionary<string, object>();
            var fields = type.GetFields(FIELDS_FLAGS);

            if (fields.Length > 0)
            {
                values = fields.Where(f =>
                {
                    bool result = f.GetValue(instance) != null;
                    if (condition != null)
                    {
                        result = result && condition(f);
                    }
                    return result;
                })
                .ToDictionary(f => f.Name, f => f.GetValue(instance));
            }

            return values;
        }

        /// <summary>
        /// Copy fields values from <paramref name="source"/> to <paramref name="target"/>
        /// </summary>
        /// <param name="source">The source instance to copy fields <see cref="Dictionary{TKey, TValue}"/> values</param>
        /// <param name="target">The target that will receive the fields <see cref="Dictionary{TKey, TValue}"/> values</param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public virtual bool CopyFieldValues(T source, T target, Predicate<FieldInfo> condition = null)
        {
            if (source == null || target == null)
            {
                return false;
            }

            Type targetType = target.GetType();
            var values = GetFieldsData(source, condition);

            if (values.Count == 0)
            {
                return false;
            }

            foreach (var data in values)
            {
                var field = targetType.GetField(data.Key, FIELDS_FLAGS);
                field.SetValue(target, data.Value);
            }

            return true;
        }
    }
}
