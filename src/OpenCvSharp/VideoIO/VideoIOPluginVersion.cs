namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// VideoIO plugin version information returned by the registry.
    /// VideoIO 注册表返回的插件版本信息。
    /// </summary>
    public readonly struct VideoIOPluginVersion
    {
        /// <summary>
        /// Initializes plugin version information.
        /// 初始化插件版本信息。
        /// </summary>
        public VideoIOPluginVersion(int abi, int api, string version)
        {
            Abi = abi;
            Api = api;
            Version = version ?? string.Empty;
        }

        /// <summary>Gets the plugin ABI version. 获取插件 ABI 版本。</summary>
        public int Abi { get; }

        /// <summary>Gets the plugin API version. 获取插件 API 版本。</summary>
        public int Api { get; }

        /// <summary>Gets the plugin version text. 获取插件版本文本。</summary>
        public string Version { get; }

        /// <summary>
        /// Returns the plugin version text.
        /// 返回插件版本文本。
        /// </summary>
        public override string ToString()
        {
            return Version;
        }
    }
}
