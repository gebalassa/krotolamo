// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("A3dvH+Zh02y2AewcQ93slyP20Ggh8n+/jmlZmgRTqy1ygdpZH1oxTFeVI1U/LhZdhXiXk1pYT/na9pD7ZtRXdGZbUF980B7QoVtXV1dTVlXUV1lWZtRXXFTUV1dWy/kLeFC9NUmunPREqhrVNLtlxCvmJzcCtlt2ev+pZBcPS3UsbNp1sllzNalUS14ejO6a1WKK9U4LtTNmV58Br1x60RKpEXIPiK05x3R2sgSoCgkJuYdo3Ahwwola/dDzCM21XotS5NdKl8vVffdJ6m21snvRQY2mMrQRNHhmV6K+p0n6QFCbPFAHQvn2dJNPRz2I08x7QdSWKeuJQ19L4/kbT4+cqOcPtdeQLUhRBQTawXvqDsW1LpV6uxgxAHf7iD0c5VRVV1ZX");
        private static int[] order = new int[] { 7,8,6,7,8,6,8,8,9,11,10,13,12,13,14 };
        private static int key = 86;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
