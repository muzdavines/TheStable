using System;
using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Used to retrieve an appropriate data validator for a requested data type.
    /// </summary>
    public static class DataValidatorResolver
    {
        private static readonly Dictionary<Type, DataValidator> DataValidatorCache 
            = new Dictionary<Type, DataValidator>();

        /// <summary>
        /// Resolves the data validator and caches it in process.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <returns></returns>
        public static DataValidator Resolve<TDataType>()
        {
            DataValidator validator;
            var type = typeof(TDataType);

            if (!DataValidatorCache.TryGetValue(type, out validator))
            {
                var validatorAttribute =
                    (DataValidatorAttribute)Attribute.GetCustomAttribute(type, typeof(DataValidatorAttribute));

                if (validatorAttribute == null)
                    return null;

                validator = (DataValidator)Activator.CreateInstance(validatorAttribute.DataValidatorType, type);
                DataValidatorCache.Add(type, validator);
            }

            return validator;
        }
    }
}