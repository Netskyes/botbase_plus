using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using java.lang;
using net.sf.jni4net;
using net.sf.jni4net.jni;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //var setup = new BridgeSetup(true);
            //setup.Verbose = true;

            ////setup.AddAllJarsClassPath("./");
            //setup.JavaHome = @"C:\Program Files (x86)\Java\jdk1.8.0_152";

            //var bridge = Bridge.CreateJVM(setup);
            //var osbotJar = AssemblyDirectory + @"\osbot.jar";

            ////var classLoader = java.lang.ClassLoader._class.getSuperclass();
            ////var addUrl = classLoader.getDeclaredMethod("addURL", new java.lang.Class[] { java.net.URL._class });
            ////addUrl.setAccessible(true);
            ////addUrl.invoke(classLoader, new java.lang.Object[] { osbot.toURL() });
            //var jniEnv = JNIEnv.ThreadEnv;
            //var jarFile = Class.forName("java.util.jar.JarFile").getConstructor
            //    (new Class[] { java.lang.String._class }).newInstance(new java.lang.Object[] { jniEnv.NewString(osbotJar) });

            //var entries = jarFile.getClass().getMethod("entries", null).invoke(jarFile, null);
            //var hasMoreElements = entries.getClass().getMethod("hasMoreElements", null);
            //var nextElement = entries.getClass().getMethod("nextElement", null);
            //var classesInJar = new List<string>();
            //while ((bool)(java.lang.Boolean)hasMoreElements.invoke(entries, null))
            //{
            //    var path = nextElement.invoke(entries, null).ToString();
            //    if (path.EndsWith(".class", StringComparison.InvariantCulture))
            //        classesInJar.Add(path.Replace(".class", "").Replace("/", "."));
            //}

            //var jarToLoad_asUrl = new java.io.File(osbotJar).toURL();
            //var urlArray = jniEnv.NewObjectArray(1, java.net.URL._class, null);
            //jniEnv.SetObjectArrayElement(urlArray, 0, jarToLoad_asUrl);

            //var urlClassLoaderClass = Class.forName("java.net.URLClassLoader");
            //var ctor = urlClassLoaderClass.getConstructors()[2];
            //var systemClassLoader = ClassLoader.getSystemClassLoader();
            //var urlClassLoader = (ClassLoader)ctor.newInstance(new java.lang.Object[] { urlArray, systemClassLoader });

            //foreach (var classInJar in classesInJar)
            //{
            //    try
            //    {
            //        urlClassLoader.loadClass(classInJar);
            //        Console.WriteLine("Class loaded: " + classInJar);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        Console.WriteLine("Failed to load: " + classInJar);
            //    }
            //}
        }

        public static string AssemblyDirectory
        {
            get
            {
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }
    }
}
