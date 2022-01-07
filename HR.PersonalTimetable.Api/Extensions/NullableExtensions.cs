namespace HR.PersonalTimetable.Api.Extensions
{
    public static class NullableExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the nullable value is either <c>null</c> or has the default value for its underlying type, otherwise returns <c>false</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrDefault<T>(this T? value) where T : struct 
            => value.GetValueOrDefault().Equals(default(T));
    }
}
