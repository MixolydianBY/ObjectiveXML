namespace ObjectiveXML.Utilities
{
    using System;
    using MEFLight;
    using MEFLight.Catalogs;

    internal class MefLightFactory
    {
        private static readonly Lazy<MefLightFactory> _instance = new Lazy<MefLightFactory>(() => new MefLightFactory());
        private DiContainer _container;

        private MefLightFactory()
        {
            Initialize();
        }

        public static MefLightFactory Singleton => _instance.Value;

        private void Initialize()
        {
            var catalog = new DirectoryCatalog();
            _container = new DiContainer(catalog);
        }

        public void ResolveImports(object instance)
        {
            _container.LoadImports(instance);
        }
    }
}
