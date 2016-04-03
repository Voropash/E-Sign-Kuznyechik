using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kuznec
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ECPoint Q = new ECPoint();
        private BigInteger d = new BigInteger();
        
        private void button3_Click(object sender, EventArgs e)
        {
            try {
                BigInteger p = new BigInteger("6277101735386680763835789423207666416083908700390324961279", 10);
                BigInteger a = new BigInteger("-3", 10);
                BigInteger b = new BigInteger("64210519e59c80e70fa7e9ab72243049feb8deecc146b9b1", 16);
                byte[] xG = Helpers.FromHexStringToByte("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012");
                BigInteger n = new BigInteger("ffffffffffffffffffffffff99def836146bc9b1b4d22831", 16);
                byte[] h0 = Helpers.FromHexStringToByte("03188da80eb03090f678dad7a8a7a9fa99acc98a9daa");
                Signer SIGNER = new Signer(p, a, b, n, xG, h0);
                d = SIGNER.GetPrivateKey(192);
                Q = SIGNER.GetPublicKey(d);
                byte[] H = Encoding.Default.GetBytes(textBox1.Text);
                string sign = SIGNER.SingIt(H, d);
                textBox2.Text = sign;
            } catch (Exception)
            {
                label1.Text = "Ошибка";
            }
}

        private void button2_Click(object sender, EventArgs e)
        {
            try {
                BigInteger p = new BigInteger("6277101735386680763835789423207666416083908700390324961279", 10);
                BigInteger a = new BigInteger("-3", 10);
                BigInteger b = new BigInteger("64210519e59c80e70fa7e9ab72243049feb8deecc146b9b1", 16);
                byte[] xG = Helpers.FromHexStringToByte("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012");
                BigInteger n = new BigInteger("ffffffffffffffffffffffff99def836146bc9b1b4d22831", 16);
                byte[] h0 = Helpers.FromHexStringToByte("03188da80eb03090f678dad7a8a7a9fa99acc98a9daa");
                Signer SIGNER = new Signer(p, a, b, n, xG, h0);
                byte[] H = Encoding.Default.GetBytes(textBox1.Text);
                bool result = SIGNER.SingVerify(H, textBox2.Text, Q);
                if (result)
                {
                    label1.Text = "Совпадает";
                } else
                {
                    label1.Text = "Отличается";
                }
            } catch (Exception)
            {
                label1.Text = "Ошибка";
            }
        }
    }
}
