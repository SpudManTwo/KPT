namespace KPT
{
    static class Version
    {
        /// <summary>
        /// List of known versions
        /// </summary>
        public enum Versions
        {
            beta_0_0_1,
            beta_1_0_0,
            beta_1_0_1,
            beta_1_1_0,
            beta_1_1_1,
            beta_1_1_2,
            beta_1_2_0
        };

        /// <summary>
        /// Current version
        /// </summary>
        const Versions CURRENT_VERSION = Versions.beta_1_2_0;

        public static Versions CurrentVersion
        {
            get
            {
                return CURRENT_VERSION;
            }

        }
        
        /// <summary>
        /// Convert the current version to a more user friendly string
        /// </summary>
        /// <returns>Current version as string formatted to current specifications</returns>
        public static string ToFormattedString(this Versions version)
        {
            string newVersion = version.ToString();
            newVersion = newVersion.Replace("_", ".");
            newVersion = newVersion.Replace("beta.", "beta-");
            return newVersion;
        }
    }
}
