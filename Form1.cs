using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace RBDv1._0
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
     
        private void button1_Click_1(object sender, EventArgs e)
        {

            //Declaring input data variables
            double fc = double.Parse(concCompStr.Text);
            double fy = double.Parse(stRftYieldStress.Text);
            double Es = double.Parse(stRftModElas.Text);
            double B = double.Parse(beamWidth.Text);
            double H = double.Parse(beamHeight.Text);
            double c = double.Parse(concCover.Text);
            double Mu = double.Parse(ultMoment.Text);
            int botBarDia = int.Parse(botBar.Text);
            int topBarDia = int.Parse(topBar.Text);
            int stirBarDia = int.Parse(stirBar.Text);
            double pi = 3.14159265358979;
            double phiMoment = 0.90;
            double ec = 0.003; // concrete strain
            double es = 0.005; // tension rft strain
            double dt = H - (c + stirBarDia + botBarDia / 2); //effective depth
            double ct = (ec / (ec + es)) * dt;
            double beta1 = 1;
            double y = 0;
            double Astension = 0;
            double AsCompression = 0;
            double Asreq = 0;

            //Check minimum concrete sompressive strength
            if (fc >= 17 && fc <= 28)
                beta1 = 0.85;
            else if (fc > 28 && fc < 55)
                beta1 = (0.85 - (0.05 * (fc - 28) / 7));
            else if (fc >= 55)
                beta1 = 0.65;
            else
            {
                MessageBox.Show("The concrete strength should not be less that 17 MPa.",
                      "Concrete Strength Alert",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);
                return;
            }

            // check singly reinforced section
            double ymax = beta1 * ct;

            double AsMaxSingl = 0.85 * fc * B * ymax / fy;

            double Mursingle = (phiMoment * 0.85 * fc * B * ymax * (dt - ymax / 2)) / 1000000;

            // check doubly reinforced section
            double dc = ct - (c + stirBarDia + (topBarDia / 2));
            double es1 = ((ct - dc) / ct) * ec;
            double fs1 = Math.Max((es1 * Es), fy);
            double Asmaxdouble = .04 * B * dt;
            double Ascomp = (Asmaxdouble * fy - 0.85 * fc * B * ymax) / (fs1 - 0.85 * fc);
            double Murdouble = (phiMoment * (0.85 * fc * B * ymax * (dt - ymax / 2)) + (Ascomp * (fs1 - 0.85 * fc) * (dt - dc))) / 1000000;

            String secStatus = "--";

            if (Mu <= Mursingle)
            {
                secStatus = "Singly Reinforced section";
                sectionStatus.Text = secStatus;
                sectionStatus.ForeColor = System.Drawing.Color.Chartreuse;
                y = dt - (Math.Sqrt(dt * dt - ((2 * Mu) / (phiMoment * 0.85 * fc * B))));
                Astension = Math.Ceiling((0.85 * fc * B * y / fy) * 1000000);
                // Checking maximum Rft.
                double Asmax = 0.04 * B * dt;
                // Checking minimum Rft.
                Double Asmin1 = 0.25 * (Math.Sqrt(fc) / fy) * B * dt;
                Double Asmin2 = (1.4 / fy) * B * dt;
                //Double Asmin3 = 4 / 3 * As;
                //Double Asmin = Math.Min(Math.Max(Asmin1, Asmin2), Asmin3);
                Double Asmin = Math.Ceiling(Math.Max(Asmin1, Asmin2));
                if (Astension < Asmin)
                    Asreq = Asmin;
                else
                    Asreq = Astension;
                reqAs.Text = Asreq.ToString();
                beamSecWidth.Text = B.ToString();
                beamSecHeight.Text = H.ToString();
                double botBarNo = Math.Ceiling(Asreq / (pi * botBarDia * botBarDia / 4));
                double topBarNo = Math.Ceiling(AsCompression / (pi * topBarDia * topBarDia / 4));
                bottomBars.Text = (botBarNo.ToString() + "ɸ" + botBarDia.ToString());
                reqAsComp.Text = ("- - -");
                compressionBars.Text = ("- - -");

            }
            else if (Mu > Mursingle && Mu <= Murdouble)
            {
                secStatus = "Doubly Reinforced section";
                sectionStatus.Text = secStatus;
                sectionStatus.ForeColor = System.Drawing.Color.Chartreuse;
                AsCompression = Math.Ceiling(((Mu * 1000000 / phiMoment) - (0.85 * fc * B * ymax * (dt - ymax / 2))) / ((fs1 - 0.85 * fc) * (dt - dc)));
                reqAsComp.Text = AsCompression.ToString();
                Astension = Math.Ceiling((((0.85 * fc * B * ymax) + AsCompression * (fs1 - 0.85 * fc)) / fy));
                // Checking maximum Rft.
                double Asmax = 0.04 * B * dt;
                // Checking minimum Rft.
                Double Asmin1 = 0.25 * (Math.Sqrt(fc) / fy) * B * dt;
                Double Asmin2 = (1.4 / fy) * B * dt;
                //Double Asmin3 = 4 / 3 * As;
                //Double Asmin = Math.Min(Math.Max(Asmin1, Asmin2), Asmin3);
                Double Asmin = Math.Ceiling(Math.Max(Asmin1, Asmin2));
                if (Astension < Asmin)
                    Asreq = Asmin;
                else
                    Asreq = Astension;
                reqAs.Text = Asreq.ToString();
                beamSecWidth.Text = B.ToString();
                beamSecHeight.Text = H.ToString();
                double botBarNo = Math.Ceiling(Asreq / (pi * botBarDia * botBarDia / 4));
                double topBarNo = Math.Ceiling(AsCompression / (pi * topBarDia * topBarDia / 4));
                bottomBars.Text = (botBarNo.ToString() + "ɸ" + botBarDia.ToString());
                compressionBars.Text = (topBarNo.ToString() + "ɸ" + topBarDia.ToString());
                //areaSteel.Text = Astension.ToString();

            }
            else
            {
                secStatus = "Section Unsafe";
                sectionStatus.Text = secStatus;
                sectionStatus.ForeColor = System.Drawing.Color.Red;
                reqAs.Text = ("- - -");
                bottomBars.Text = ("- - -");
                reqAsComp.Text = ("- - -");
                compressionBars.Text = ("- - -");
                return;
            }


        }



        private void button3_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }




        private void button4_Click_1(object sender, EventArgs e)
        {

            //Project info
            string projName = textBox11.Text;
            string floor = textBox12.Text;
            string beamName = textBox13.Text;
            string secNo = textBox14.Text;

            //Declaring input data variables
            double fc = double.Parse(concCompStr.Text);
            double fy = double.Parse(stRftYieldStress.Text);
            double Es = double.Parse(stRftModElas.Text);
            double B = double.Parse(beamWidth.Text);
            double H = double.Parse(beamHeight.Text);
            double c = double.Parse(concCover.Text);
            double Mu = double.Parse(ultMoment.Text);
            //double Vu = double.Parse(ultShear.Text);
            //double Tu = double.Parse(ultTorsion.Text);
            int botBarDia = int.Parse(botBar.Text);
            int topBarDia = int.Parse(topBar.Text);
            //int sideBarDia = int.Parse(sideBar.Text);
            int stirBarDia = int.Parse(stirBar.Text);
            //int stirBarNo = int.Parse(StirBranchNumber.Text);
            //int topBarNo = 2;
            double pi = 3.14159265358979;
            double phiMoment = 0.90;
            //double phiShear = 0.75;
            double ec = 0.003; // concrete strain
            double es = 0.005; // tension rft strain
            double dt = H - (c + stirBarDia + botBarDia / 2); //effective depth
            double ct = (ec / (ec + es)) * dt;
            double beta1 = 1;
            double y = 0;
            double Astension = 0;
            double AsCompression = 0;
            double Asreq = 0;

            //Check minimum concrete sompressive strength
            if (fc >= 17 && fc <= 28)
                beta1 = 0.85;
            else if (fc > 28 && fc < 55)
                beta1 = (0.85 - (0.05 * (fc - 28) / 7));
            else if (fc >= 55)
                beta1 = 0.65;
            else
            {
                MessageBox.Show("The concrete strength should not be less that 17 MPa.",
                      "Concrete Strength Alert",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);
                return;
            }

            // check singly reinforced section
            double ymax = beta1 * ct;

            double AsMaxSingl = 0.85 * fc * B * ymax / fy;

            double Mursingle = (phiMoment * 0.85 * fc * B * ymax * (dt - ymax / 2)) / 1000000;

            // check doubly reinforced section
            double dc = ct - (c + stirBarDia + (topBarDia / 2));
            double es1 = ((ct - dc) / ct) * ec;
            double fs1 = Math.Max((es1 * Es), fy);
            double Asmaxdouble = .04 * B * dt;
            double Ascomp = (Asmaxdouble * fy - 0.85 * fc * B * ymax) / (fs1 - 0.85 * fc);
            double Murdouble = (phiMoment * (0.85 * fc * B * ymax * (dt - ymax / 2)) + (Ascomp * (fs1 - 0.85 * fc) * (dt - dc))) / 1000000;

            String secStatus = "--";

            if (Mu <= Mursingle)
            {
                secStatus = "Singly Reinforced section";
                sectionStatus.Text = secStatus;
                sectionStatus.ForeColor = System.Drawing.Color.Chartreuse;
                y = dt - (Math.Sqrt(dt * dt - ((2 * Mu) / (phiMoment * 0.85 * fc * B))));
                Astension = Math.Ceiling((0.85 * fc * B * y / fy) * 1000000);
                // Checking maximum Rft.
                double Asmax = 0.04 * B * dt;
                // Checking minimum Rft.
                Double Asmin1 = 0.25 * (Math.Sqrt(fc) / fy) * B * dt;
                Double Asmin2 = (1.4 / fy) * B * dt;
                //Double Asmin3 = 4 / 3 * As;
                //Double Asmin = Math.Min(Math.Max(Asmin1, Asmin2), Asmin3);
                Double Asmin = Math.Ceiling(Math.Max(Asmin1, Asmin2));
                if (Astension < Asmin)
                    Asreq = Asmin;
                else
                    Asreq = Astension;
                reqAs.Text = Asreq.ToString();
                beamSecWidth.Text = B.ToString();
                beamSecHeight.Text = H.ToString();
                double botBarNo = Math.Ceiling(Asreq / (pi * botBarDia * botBarDia / 4));
                double topBarNo = Math.Ceiling(AsCompression / (pi * topBarDia * topBarDia / 4));
                bottomBars.Text = (botBarNo.ToString() + "ɸ" + botBarDia.ToString());
                reqAsComp.Text = ("- - -");
                compressionBars.Text = ("- - -");

                //areaSteel.Text = Astension.ToString();

            }
            else if (Mu > Mursingle && Mu <= Murdouble)
            {
                secStatus = "Doubly Reinforced section";
                sectionStatus.Text = secStatus;
                sectionStatus.ForeColor = System.Drawing.Color.Chartreuse;
                AsCompression = Math.Ceiling(((Mu * 1000000 / phiMoment) - (0.85 * fc * B * ymax * (dt - ymax / 2))) / ((fs1 - 0.85 * fc) * (dt - dc)));
                reqAsComp.Text = AsCompression.ToString();
                Astension = Math.Ceiling((((0.85 * fc * B * ymax) + AsCompression * (fs1 - 0.85 * fc)) / fy));
                // Checking maximum Rft.
                double Asmax = 0.04 * B * dt;
                // Checking minimum Rft.
                Double Asmin1 = 0.25 * (Math.Sqrt(fc) / fy) * B * dt;
                Double Asmin2 = (1.4 / fy) * B * dt;
                //Double Asmin3 = 4 / 3 * As;
                //Double Asmin = Math.Min(Math.Max(Asmin1, Asmin2), Asmin3);
                Double Asmin = Math.Ceiling(Math.Max(Asmin1, Asmin2));
                if (Astension < Asmin)
                    Asreq = Asmin;
                else
                    Asreq = Astension;
                reqAs.Text = Asreq.ToString();
                beamSecWidth.Text = B.ToString();
                beamSecHeight.Text = H.ToString();
                double botBarNo = Math.Ceiling(Asreq / (pi * botBarDia * botBarDia / 4));
                double topBarNo = Math.Ceiling(AsCompression / (pi * topBarDia * topBarDia / 4));
                bottomBars.Text = (botBarNo.ToString() + "ɸ" + botBarDia.ToString());
                compressionBars.Text = (topBarNo.ToString() + "ɸ" + topBarDia.ToString());
                //areaSteel.Text = Astension.ToString();

            }
            else
            {
                secStatus = "Section Unsafe";
                sectionStatus.Text = secStatus;
                sectionStatus.ForeColor = System.Drawing.Color.Red;
                reqAs.Text = ("- - -");
                bottomBars.Text = ("- - -");
                reqAsComp.Text = ("- - -");
                compressionBars.Text = ("- - -");
                //return;
            }
            //******************************* pdf *********************************

            var titleFont = FontFactory.GetFont("VERDANA", 14.0f, BaseColor.BLACK);
            var titleFont1 = FontFactory.GetFont("VERDANA", 14.0f, 1, BaseColor.BLACK);
            var titleFont2 = FontFactory.GetFont("VERDANA", 12.0f, 1, BaseColor.BLACK);
            var itemFont = FontFactory.GetFont("VERDANA", 12.0f, BaseColor.BLUE);
            var subItemFont = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.DARK_GRAY);
            var elementFont = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.BLACK);
            var f1 =  FontFactory.GetFont("VERDANA", 4.0f, BaseColor.WHITE);
            var f2 = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.GREEN);
            var f4 = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.RED);
            var f3 = FontFactory.GetFont("VERDANA", 11.0f, BaseColor.BLACK);


            //Full path to the Unicode Arial file
            string ARIALUNI_TFF = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIALUNI.TTF");

            //Create a base font object making sure to specify IDENTITY-H
            BaseFont bf = BaseFont.CreateFont(ARIALUNI_TFF, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            //Create a specific font object
            iTextSharp.text.Font f = new iTextSharp.text.Font(bf, 10);

            string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Beam " + beamName + " Sec " + secNo + " Flexural Design Report.pdf");


            Document report = new Document(iTextSharp.text.PageSize.A4, 35, 35, 35, 35);

            PdfWriter wri = PdfWriter.GetInstance(report, new FileStream(outputFile, FileMode.Create));
            report.Open();

            var content = wri.DirectContent;
            var pageBorderRect = new iTextSharp.text.Rectangle(report.PageSize);

            pageBorderRect.Left += report.LeftMargin;
            pageBorderRect.Right -= report.RightMargin;
            pageBorderRect.Top -= report.TopMargin;
            pageBorderRect.Bottom += report.BottomMargin;

            content.SetColorStroke(BaseColor.BLACK);
            content.Rectangle(pageBorderRect.Left, pageBorderRect.Bottom, pageBorderRect.Width, pageBorderRect.Height);
            content.Stroke();

           
        PdfPTable info = new PdfPTable(5);
            info.HorizontalAlignment = 0;
            info.DefaultCell.FixedHeight = 20f;
            info.TotalWidth = 525f;
            info.LockedWidth = true;
            float[] widths = new float[] { 70f, 50f, 60f, 50f, 60f };
            info.SetWidths(widths);

            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("logo.png");
            PdfPCell cell = new PdfPCell(logo);
            cell.Rowspan = 2;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;

            info.AddCell(cell);
            info.AddCell("Project name:");
            info.AddCell(projName);
            info.AddCell("Beam floor  :");
            info.AddCell(floor);
            info.AddCell("Beam Number :");
            info.AddCell(beamName);
            info.AddCell("Section No. :");
            info.AddCell(secNo);
            info.HorizontalAlignment = Element.ALIGN_CENTER;
            report.Add(info);

            //Title
            Paragraph Title = new Paragraph("Rectangular Concrete Beam Section Design", titleFont1);
            Paragraph Title1 = new Paragraph("Flexural design - according to ACI318M-14", titleFont2);

            Title.Alignment = Element.ALIGN_CENTER;
            Title.IndentationRight = 100;
            Title.IndentationLeft = 100;

            Title1.Alignment = Element.ALIGN_CENTER;
            Title1.IndentationRight = 100;
            Title1.IndentationLeft = 100;

            report.Add(Title);
            report.Add(Title1);


            iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance("Diagram_report.png");
            pic.Alignment = Element.ALIGN_CENTER;

            report.Add(pic);

            //Input data
            Paragraph inputTitle = new Paragraph("Input data", itemFont);
            report.Add(inputTitle);

            Paragraph materialData = new Paragraph("Material data", subItemFont);
            report.Add(materialData);

            Paragraph space = new Paragraph(" ", f1);
            report.Add(space);

            PdfPTable material = new PdfPTable(6);

            material.HorizontalAlignment = 0;
            //material.DefaultCell.FixedHeight = 20f;
            material.TotalWidth = 525f;
            material.LockedWidth = true;
            float[] widths1 = new float[] { 80f, 20f, 5f, 20f, 15f, 70f };
            material.SetWidths(widths1);
            material.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


            material.DefaultCell.BorderColor=BaseColor.WHITE;

            material.AddCell(new PdfPCell(new Phrase("Concrte compressive strength", f)));
            material.AddCell(new PdfPCell(new Phrase("f'c", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(fc.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase("MPa", f)));
            material.AddCell(new PdfPCell(new Phrase("[Cylinder]", f)));

            material.AddCell(new PdfPCell(new Phrase("Reinforcement steel yield stress", f)));
            material.AddCell(new PdfPCell(new Phrase("fy", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(fy.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase("MPa", f)));
            material.AddCell(new PdfPCell(new Phrase(" ", f)));

            material.AddCell(new PdfPCell(new Phrase("Reinforcement steel modulus of elasticity", f)));
            material.AddCell(new PdfPCell(new Phrase("Es", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(Es.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase("MPa", f)));
            material.AddCell(new PdfPCell(new Phrase(" ", f)));

            material.AddCell(new PdfPCell(new Phrase("Strength reduction factor", f)));
            material.AddCell(new PdfPCell(new Phrase("ɸ", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(phiMoment.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase(" ", f)));
            material.AddCell(new PdfPCell(new Phrase("[Flexure - tension controlled]", f)));

            
            report.Add(material);

            //report.Add(space);

            Paragraph sectionData = new Paragraph("Section data", subItemFont);
            report.Add(sectionData);

            report.Add(space);

            PdfPTable section = new PdfPTable(6);

            section.HorizontalAlignment = 0;
            section.DefaultCell.FixedHeight = 20f;
            section.TotalWidth = 525f;
            section.LockedWidth = true;
            section.SetWidths(widths1);
            section.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

            section.AddCell(new PdfPCell(new Phrase("Section width", f)));
            section.AddCell(new PdfPCell(new Phrase("b", f)));
            section.AddCell(new PdfPCell(new Phrase("=", f)));
            section.AddCell(new PdfPCell(new Phrase(B.ToString(), f)));
            section.AddCell(new PdfPCell(new Phrase("mm", f)));
            section.AddCell(new PdfPCell(new Phrase(" ", f)));

            section.AddCell(new PdfPCell(new Phrase("Section thickness", f)));
            section.AddCell(new PdfPCell(new Phrase("h", f)));
            section.AddCell(new PdfPCell(new Phrase("=", f)));
            section.AddCell(new PdfPCell(new Phrase(H.ToString(), f)));
            section.AddCell(new PdfPCell(new Phrase("mm", f)));
            section.AddCell(new PdfPCell(new Phrase(" ", f)));

            section.AddCell(new PdfPCell(new Phrase("Concrete cover", f)));
            section.AddCell(new PdfPCell(new Phrase("c", f)));
            section.AddCell(new PdfPCell(new Phrase("=", f)));
            section.AddCell(new PdfPCell(new Phrase(c.ToString(), f)));
            section.AddCell(new PdfPCell(new Phrase("mm", f)));
            section.AddCell(new PdfPCell(new Phrase(" ", f)));

            report.Add(section);


            report.Add(space);

            Paragraph strainingAction = new Paragraph("Straining action", subItemFont);
            report.Add(strainingAction);

            report.Add(space);


            PdfPTable Straining = new PdfPTable(6);

            Straining.HorizontalAlignment = 0;
            Straining.DefaultCell.FixedHeight = 20f;
            Straining.TotalWidth = 525f;
            Straining.LockedWidth = true;
            Straining.SetWidths(widths1);
            Straining.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

            Straining.AddCell(new PdfPCell(new Phrase("Ultimate moment", f)));
            Straining.AddCell(new PdfPCell(new Phrase("Mu", f)));
            Straining.AddCell(new PdfPCell(new Phrase("=", f)));
            Straining.AddCell(new PdfPCell(new Phrase(Mu.ToString(), f)));
            Straining.AddCell(new PdfPCell(new Phrase("kN.m", f)));
            Straining.AddCell(new PdfPCell(new Phrase(" ", f)));

            report.Add(Straining);



            //Design Calculations
            Paragraph calcTitle = new Paragraph("Section design calculations", itemFont);
            report.Add(calcTitle);
            //report.Add(space);

            
            //------------------------------ Section status -------------------------------------
            if (Mu <= Mursingle)
            {

                Paragraph SecDesStatus = new Paragraph("Section is designed as Singly reinforced section", f2);
                report.Add(SecDesStatus);

                report.Add(space);

                PdfPTable single = new PdfPTable(6);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                double ysingle = Math.Round((y * 1000000), 2);


                single.AddCell(new PdfPCell(new Phrase("Concrete compression strain", f)));
                single.AddCell(new PdfPCell(new Phrase("ec", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("0.003", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Steel tensile strain", f)));
                single.AddCell(new PdfPCell(new Phrase("et", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("0.005", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Section depth", f)));
                single.AddCell(new PdfPCell(new Phrase("d", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(dt.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d=h-c-Østirrup-Øtension/2]", f)));

                single.AddCell(new PdfPCell(new Phrase("Neutral axis depth", f)));
                single.AddCell(new PdfPCell(new Phrase("X", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(ct.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[(ec/(ec+es))*dt]", f)));

                single.AddCell(new PdfPCell(new Phrase("Equivelent compression block depth", f)));
                single.AddCell(new PdfPCell(new Phrase("a", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(ysingle.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[a = d-{√(d*d-[(2*Mu)/(ɸ*0.85*f'c*b)])]", f)));

                single.AddCell(new PdfPCell(new Phrase("Required area of steel", f)));
                single.AddCell(new PdfPCell(new Phrase("As(req)", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Astension.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[As(req) = 0.85*f'c*b*X / fy]", f)));

                // Checking minimum Rft.
                Double Asmin1 = 0.25 * (Math.Sqrt(fc) / fy) * B * dt;
                Double Asmin2 = (1.4 / fy) * B * dt;
                //Double Asmin3 = 4 / 3 * As;
                //Double Asmin = Math.Min(Math.Max(Asmin1, Asmin2), Asmin3);
                Double Asmin = Math.Ceiling(Math.Max(Asmin1, Asmin2));

                single.AddCell(new PdfPCell(new Phrase("Minimum area steel", f)));
                single.AddCell(new PdfPCell(new Phrase("Asmin", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Asmin.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[As(min) = maximum of:]                    [Asminl = 0.25*[sqrt(f'c)/ fy]* b*d]    [Asmin2 = (1.4 / fy)*a*d]", f)));

                // Checking maximum Rft.
                double Asmax = 0.04 * B * dt;

                single.AddCell(new PdfPCell(new Phrase("Maximum area steel", f)));
                single.AddCell(new PdfPCell(new Phrase("Asmax", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Asmax.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[Asmax = 0.04*b*d]", f)));

                report.Add(single);
                //report.Add(space);
                if (Astension < Asmin)
                    Asreq = Asmin;
                else
                    Asreq = Astension;

                double botBarNo = Math.Ceiling(Asreq / (pi * botBarDia * botBarDia / 4));
                double topBarNo = Math.Ceiling(AsCompression / (pi * topBarDia * topBarDia / 4));

                Paragraph secOutput1 = new Paragraph("Concrete section:    " + B + "mm * " + H + "mm" , f3);
                report.Add(secOutput1);

                Paragraph secOutput2 = new Paragraph("Tension Rft.        :    " + botBarNo + " Ø " + botBarDia , f3);
                report.Add(secOutput2);

                

            }
            else if (Mu > Mursingle && Mu <= Murdouble)
            {

                Paragraph SecDesStatus = new Paragraph("Section is designed as Doubly reinforced section", f2);
                report.Add(SecDesStatus);

                report.Add(space);

                PdfPTable single = new PdfPTable(6);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


                single.AddCell(new PdfPCell(new Phrase("Concrete compression strain", f)));
                single.AddCell(new PdfPCell(new Phrase("ec", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("0.003", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Steel tensile strain", f)));
                single.AddCell(new PdfPCell(new Phrase("et", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("0.005", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Section depth", f)));
                single.AddCell(new PdfPCell(new Phrase("d", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(dt.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d=h-c-Østirrup-Øtension/2]", f)));

                double d1 = c + stirBarDia + (topBarDia / 2);

                single.AddCell(new PdfPCell(new Phrase("Depth of compression steel", f)));
                single.AddCell(new PdfPCell(new Phrase("d'", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(d1.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d'=c+Østirrup+Øcompression/2]", f)));

                single.AddCell(new PdfPCell(new Phrase("Neutral axis depth", f)));
                single.AddCell(new PdfPCell(new Phrase("X", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(ct.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[(ec/(ec+es))*dt]", f)));

                double beta = Math.Round(beta1, 3);

                single.AddCell(new PdfPCell(new Phrase("Equivelent depth factor", f)));
                single.AddCell(new PdfPCell(new Phrase("β", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(beta.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                double ydouble = Math.Round(ymax, 0);

                single.AddCell(new PdfPCell(new Phrase("Equivelent compression block depth", f)));
                single.AddCell(new PdfPCell(new Phrase("a", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(ydouble.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[a = β * X)]", f)));

                single.AddCell(new PdfPCell(new Phrase("Required area of tension steel", f)));
                single.AddCell(new PdfPCell(new Phrase("As(req)", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Astension.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[As(req) = 0.85*f'c*b*X / fy]", f)));

                double Asc = Math.Round(AsCompression, 0);

                single.AddCell(new PdfPCell(new Phrase("Required area of compression steel", f)));
                single.AddCell(new PdfPCell(new Phrase("As(comp)", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Asc.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[Asc={(Mu/Ø)-(0.85*f'c*b*X*[d-X/2])}/{(f's-0.85*f'c)*[d-dc]}]", f)));

                // Checking minimum Rft.
                Double Asmin1 = 0.25 * (Math.Sqrt(fc) / fy) * B * dt;
                Double Asmin2 = (1.4 / fy) * B * dt;
                //Double Asmin3 = 4 / 3 * As;
                //Double Asmin = Math.Min(Math.Max(Asmin1, Asmin2), Asmin3);
                Double Asmin = Math.Ceiling(Math.Max(Asmin1, Asmin2));

                single.AddCell(new PdfPCell(new Phrase("Minimum area steel", f)));
                single.AddCell(new PdfPCell(new Phrase("Asmin", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Asmin.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[As(min) = maximum of:]                    [Asminl = 0.25*[sqrt(f'c)/ fy]* b*d]    [Asmin2 = (1.4 / fy)*a*d]", f)));

                // Checking maximum Rft.
                double Asmax = 0.04 * B * dt;

                single.AddCell(new PdfPCell(new Phrase("Maximum area steel", f)));
                single.AddCell(new PdfPCell(new Phrase("Asmax", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Asmax.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[Asmax = 0.04*b*d]", f)));

                report.Add(single);
                report.Add(space);
                if (Astension < Asmin)
                    Asreq = Asmin;
                else
                    Asreq = Astension;

                AsCompression = Math.Ceiling(((Mu * 1000000 / phiMoment) - (0.85 * fc * B * ymax * (dt - ymax / 2))) / ((fs1 - 0.85 * fc) * (dt - dc)));
                reqAsComp.Text = AsCompression.ToString();
                Astension = Math.Ceiling((((0.85 * fc * B * ymax) + AsCompression * (fs1 - 0.85 * fc)) / fy));

                double botBarNo = Math.Ceiling(Asreq / (pi * botBarDia * botBarDia / 4));
                double topBarNo = Math.Ceiling(AsCompression / (pi * topBarDia * topBarDia / 4));

                Paragraph secOutput1 = new Paragraph("Concrete section :    " + B + "mm * " + H + "mm", f3);
                report.Add(secOutput1);

                Paragraph secOutput2 = new Paragraph("Tension Rft.         :    " + botBarNo + " Ø " + botBarDia, f3);
                report.Add(secOutput2);

                Paragraph secOutput3 = new Paragraph("Compression Rft.:    " + topBarNo + " Ø " + topBarDia, f3);
                report.Add(secOutput3);

            }
            else
            {

                report.Add(space);

                PdfPTable single = new PdfPTable(6);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


                single.AddCell(new PdfPCell(new Phrase("Concrete compression strain", f)));
                single.AddCell(new PdfPCell(new Phrase("ec", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("0.003", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Steel tensile strain", f)));
                single.AddCell(new PdfPCell(new Phrase("et", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("0.005", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Section depth", f)));
                single.AddCell(new PdfPCell(new Phrase("d", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(dt.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d=h-c-Østirrup-Øtension/2]", f)));

                double d1 = c + stirBarDia + (topBarDia / 2);

                single.AddCell(new PdfPCell(new Phrase("Depth of compression steel", f)));
                single.AddCell(new PdfPCell(new Phrase("d'", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(d1.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d'=c+Østirrup+Øcompression/2]", f)));

                single.AddCell(new PdfPCell(new Phrase("Neutral axis depth", f)));
                single.AddCell(new PdfPCell(new Phrase("X", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(ct.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[(ec/(ec+es))*dt]", f)));

                double beta = Math.Round(beta1, 3);

                single.AddCell(new PdfPCell(new Phrase("Equivelent depth factor", f)));
                single.AddCell(new PdfPCell(new Phrase("β", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(beta.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                double ydouble = Math.Round(ymax, 0);

                single.AddCell(new PdfPCell(new Phrase("Equivelent compression block depth", f)));
                single.AddCell(new PdfPCell(new Phrase("a", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(ydouble.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[a = β * X)]", f)));
                Paragraph SecDesStatus = new Paragraph("Section is UNSAFE", f4);
                report.Add(SecDesStatus);
                //return;
            }


            report.Close();

            MessageBox.Show("PDF report created on Desktop","BeaD 1.1", MessageBoxButtons.OK, MessageBoxIcon.Information);


        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://khaledfayed.co.nr";

            var si = new ProcessStartInfo(url);
            Process.Start(si);
            linkLabel1.LinkVisited = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label66_Click(object sender, EventArgs e)
        {

        }

        private void groupBox9_Enter(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

            //Declaring input data variables
            double fc = double.Parse(textBox7.Text);
            double fy = double.Parse(textBox6.Text);
            double Es = double.Parse(textBox5.Text);
            double B = double.Parse(textBox4.Text);
            double H = double.Parse(textBox3.Text);
            double c = double.Parse(textBox2.Text);
            double Vu = double.Parse(textBox1.Text);
           
            int stirBarDia = int.Parse(comboBox1.Text);
            int stirBranchNo = int.Parse(textBox8.Text);
            double pi = 3.14159265358979;
            double phiShear = 0.75;
            double lamda = 1.0;
            double d = H - (c + stirBarDia);


            // Concrete section capacity
            double Vc = (0.17 * lamda * Math.Sqrt(fc) * B * d)/1000;
            double Vcn = phiShear * Vc;

            double Vsmax = (0.66 * Math.Sqrt(fc) * B * d)/1000;

            double Vsmaxn = phiShear * Vsmax;

            double Vsecmax = Vcn + Vsmaxn;
            
            Double SPmax1 = 600;
            Double SPmax2 = d / 2;
            Double SPmax = Math.Min(SPmax1, SPmax2);
            Double SPmaxround = (Math.Ceiling(SPmax / 25)) * 25;
            String secStatus = "--";

            if (Vu <= Vcn)
            {
                secStatus = "No shear reinforcement required";
                label48.Text = secStatus;
                label48.ForeColor = System.Drawing.Color.Chartreuse;
                // Checking maximum shear Rft spacing.
                label61.Text = B.ToString();
                label59.Text = H.ToString();
                label63.Text = stirBranchNo + " branch Φ " + stirBarDia + "/" + SPmaxround + " mm";
            
            }
            else if (Vu > Vcn && Vu <= Vsecmax)
            {
                secStatus = "Shear Rft. required";
                label48.Text = secStatus;
                label48.ForeColor = System.Drawing.Color.Chartreuse;
                label61.Text = B.ToString();
                label59.Text = H.ToString();


                // Checking minimum Rft.

                double Vs = ((Vu - Vcn) / phiShear)*1000;
                double Avprov = stirBranchNo * (pi * stirBarDia * stirBarDia / 4);
                

                double Avmin1 = 0.062 * Math.Sqrt(fc) * B * SPmaxround / fy;
                double Avmin2 = 0.35 * B * SPmaxround / fy;

                double Avmin = Math.Max(Avmin1, Avmin2);
                double Av = Math.Max(Avprov, Avmin);

                double S = Math.Ceiling(Av * fy * d / Vs);

                double Sprov = (Math.Round(S / 25)) * 25;

                label63.Text = stirBranchNo + " branch Φ " + stirBarDia + "/" + Sprov + " mm";
            }
            else
            {
                secStatus = "Section Unsafe";
                label48.Text = secStatus;
                label48.ForeColor = System.Drawing.Color.Red;
                label63.Text = " -  -  -";
                //return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            //Declaring input data variables
            double fc = double.Parse(textBox7.Text);
            double fy = double.Parse(textBox6.Text);
            double Es = double.Parse(textBox5.Text);
            double B = double.Parse(textBox4.Text);
            double H = double.Parse(textBox3.Text);
            double c = double.Parse(textBox2.Text);
            double Vu = double.Parse(textBox1.Text);

            int stirBarDia = int.Parse(comboBox1.Text);
            int stirBranchNo = int.Parse(textBox8.Text);
            double pi = 3.14159265358979;
            double phiShear = 0.75;
            double lamda = 1.0;
            double d = H - (c + stirBarDia);


            // Concrete section capacity
            double Vc = (0.17 * lamda * Math.Sqrt(fc) * B * d) / 1000;
            double Vcn = phiShear * Vc;

            double Vsmax = (0.66 * Math.Sqrt(fc) * B * d) / 1000;

            double Vsmaxn = phiShear * Vsmax;

            double Vsecmax = Vcn + Vsmaxn;

            Double SPmax1 = 600;
            Double SPmax2 = d / 2;
            Double SPmax = Math.Min(SPmax1, SPmax2);
            Double SPmaxround = (Math.Ceiling(SPmax / 25)) * 25;
            String secStatus = "--";

            if (Vu <= Vcn)
            {
                secStatus = "No shear reinforcement required";
                label48.Text = secStatus;
                label48.ForeColor = System.Drawing.Color.Chartreuse;
                // Checking maximum shear Rft spacing.
                label61.Text = B.ToString();
                label59.Text = H.ToString();
                label63.Text = stirBranchNo + " branch Φ " + stirBarDia + "/" + SPmaxround + " mm";

            }
            else if (Vu > Vcn && Vu <= Vsecmax)
            {
                secStatus = "Shear Rft. required";
                label48.Text = secStatus;
                label48.ForeColor = System.Drawing.Color.Chartreuse;
                label61.Text = B.ToString();
                label59.Text = H.ToString();


                // Checking minimum Rft.

                double Vs = ((Vu - Vcn) / phiShear) * 1000;
                double Avprov = stirBranchNo * (pi * stirBarDia * stirBarDia / 4);


                double Avmin1 = 0.062 * Math.Sqrt(fc) * B * SPmaxround / fy;
                double Avmin2 = 0.35 * B * SPmaxround / fy;

                double Avmin = Math.Max(Avmin1, Avmin2);
                double Av = Math.Max(Avprov, Avmin);

                double S = Math.Ceiling(Av * fy * d / Vs);

                double Sprov = (Math.Round(S / 25)) * 25;

                label63.Text = stirBranchNo + " branch Φ " + stirBarDia + "/" + Sprov + " mm";
            }
            else
            {
                secStatus = "Section Unsafe";
                label48.Text = secStatus;
                label48.ForeColor = System.Drawing.Color.Red;
                label63.Text = " -  -  -";
                //return;
            }

            //******************************************* pdf *******************************************************

            // ******* fonts  *******
            var titleFont = FontFactory.GetFont("VERDANA", 14.0f, BaseColor.BLACK);
            var titleFont1 = FontFactory.GetFont("VERDANA", 14.0f, 1, BaseColor.BLACK);
            var titleFont2 = FontFactory.GetFont("VERDANA", 12.0f, 1, BaseColor.BLACK);
            var itemFont = FontFactory.GetFont("VERDANA", 12.0f, BaseColor.BLUE);
            var subItemFont = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.DARK_GRAY);
            var elementFont = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.BLACK);
            var f1 = FontFactory.GetFont("VERDANA", 4.0f, BaseColor.WHITE);
            var f2 = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.GREEN);
            var f4 = FontFactory.GetFont("VERDANA", 10.0f, BaseColor.RED);
            var f3 = FontFactory.GetFont("VERDANA", 11.0f, BaseColor.BLACK);
            //Full path to the Unicode Arial file
            string ARIALUNI_TFF = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIALUNI.TTF");

            //Create a base font object making sure to specify IDENTITY-H
            BaseFont bf = BaseFont.CreateFont(ARIALUNI_TFF, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            //Create a specific font object
            iTextSharp.text.Font f = new iTextSharp.text.Font(bf, 10);
            
            //**************************

            //string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Beam " + textBox13 + " Sec " + textBox14 + " Shear Design Report.pdf");

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string text13 = textBox13.Text;
            string text14 = textBox14.Text;
            string fileName = "\\Beam" + text13 + " Sec " + text14 + " Shear Design Report.pdf";
            string outputFile = path + fileName;

            
            Document report1 = new Document(iTextSharp.text.PageSize.A4, 35, 35, 35, 35);


              PdfWriter wri = PdfWriter.GetInstance(report1, new FileStream(outputFile, FileMode.Create)); 

            report1.Open();

            var content = wri.DirectContent;
            var pageBorderRect = new iTextSharp.text.Rectangle(report1.PageSize);

            pageBorderRect.Left += report1.LeftMargin;
            pageBorderRect.Right -= report1.RightMargin;
            pageBorderRect.Top -= report1.TopMargin;
            pageBorderRect.Bottom += report1.BottomMargin;

            content.SetColorStroke(BaseColor.BLACK);
            content.Rectangle(pageBorderRect.Left, pageBorderRect.Bottom, pageBorderRect.Width, pageBorderRect.Height);
            content.Stroke();


            PdfPTable info = new PdfPTable(5);
            info.HorizontalAlignment = 0;
            info.DefaultCell.FixedHeight = 20f;
            info.TotalWidth = 525f;
            info.LockedWidth = true;
            float[] widths = new float[] { 70f, 50f, 60f, 50f, 60f };
            info.SetWidths(widths);

            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("logo.png");
            PdfPCell cell = new PdfPCell(logo);
            cell.Rowspan = 2;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;

            info.AddCell(cell);
            info.AddCell("Project name:");
            info.AddCell(textBox11.Text);
            info.AddCell("Beam floor  :");
            info.AddCell(textBox12.Text);
            info.AddCell("Beam Number :");
            info.AddCell(textBox13.Text);
            info.AddCell("Section No. :");
            info.AddCell(textBox14.Text);
            info.HorizontalAlignment = Element.ALIGN_CENTER;
            report1.Add(info);

            //Title
            Paragraph Title = new Paragraph("Rectangular Concrete Beam Section Design", titleFont1);
            Paragraph Title1 = new Paragraph("Shear design - according to ACI318-14", titleFont2);
            Title.Alignment = Element.ALIGN_CENTER;
            Title1.Alignment = Element.ALIGN_CENTER;
            Title.IndentationRight = 100;
            Title.IndentationLeft = 100;
            

            report1.Add(Title);
            report1.Add(Title1);


            iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance("Diagram_report.png");
            pic.Alignment = Element.ALIGN_CENTER;

            report1.Add(pic);

            //Input data
            Paragraph inputTitle = new Paragraph("Input data", itemFont);
            report1.Add(inputTitle);

            Paragraph materialData = new Paragraph("Material data", subItemFont);
            report1.Add(materialData);

            Paragraph space = new Paragraph(" ", f1);
            report1.Add(space);

            PdfPTable material = new PdfPTable(6);

            material.HorizontalAlignment = 0;
            //material.DefaultCell.FixedHeight = 20f;
            material.TotalWidth = 525f;
            material.LockedWidth = true;
            float[] widths1 = new float[] { 70f, 20f, 5f, 20f, 15f, 70f };
            material.SetWidths(widths1);
            material.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


            material.DefaultCell.BorderColor = BaseColor.WHITE;

            material.AddCell(new PdfPCell(new Phrase("Concrte compressive strength", f)));
            material.AddCell(new PdfPCell(new Phrase("f'c", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(fc.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase("MPa", f)));
            material.AddCell(new PdfPCell(new Phrase("[Cylinder]", f)));

            material.AddCell(new PdfPCell(new Phrase("Reinforcement steel yield stress", f)));
            material.AddCell(new PdfPCell(new Phrase("fy", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(fy.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase("MPa", f)));
            material.AddCell(new PdfPCell(new Phrase(" ", f)));

            material.AddCell(new PdfPCell(new Phrase("Reinforcement steel modulus of elasticity", f)));
            material.AddCell(new PdfPCell(new Phrase("Es", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(Es.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase("MPa", f)));
            material.AddCell(new PdfPCell(new Phrase(" ", f)));

            material.AddCell(new PdfPCell(new Phrase("Strength reduction factor", f)));
            material.AddCell(new PdfPCell(new Phrase("ɸ", f)));
            material.AddCell(new PdfPCell(new Phrase("=", f)));
            material.AddCell(new PdfPCell(new Phrase(phiShear.ToString(), f)));
            material.AddCell(new PdfPCell(new Phrase(" ", f)));
            material.AddCell(new PdfPCell(new Phrase("[Shear]", f)));


            report1.Add(material);

            report1.Add(space);

            Paragraph sectionData = new Paragraph("Section data", subItemFont);
            report1.Add(sectionData);

            report1.Add(space);

            PdfPTable section = new PdfPTable(6);

            section.HorizontalAlignment = 0;
            section.DefaultCell.FixedHeight = 20f;
            section.TotalWidth = 525f;
            section.LockedWidth = true;
            section.SetWidths(widths1);
            section.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

            section.AddCell(new PdfPCell(new Phrase("Section width", f)));
            section.AddCell(new PdfPCell(new Phrase("b", f)));
            section.AddCell(new PdfPCell(new Phrase("=", f)));
            section.AddCell(new PdfPCell(new Phrase(B.ToString(), f)));
            section.AddCell(new PdfPCell(new Phrase("mm", f)));
            section.AddCell(new PdfPCell(new Phrase(" ", f)));

            section.AddCell(new PdfPCell(new Phrase("Section thickness", f)));
            section.AddCell(new PdfPCell(new Phrase("h", f)));
            section.AddCell(new PdfPCell(new Phrase("=", f)));
            section.AddCell(new PdfPCell(new Phrase(H.ToString(), f)));
            section.AddCell(new PdfPCell(new Phrase("mm", f)));
            section.AddCell(new PdfPCell(new Phrase(" ", f)));

            section.AddCell(new PdfPCell(new Phrase("Concrete cover", f)));
            section.AddCell(new PdfPCell(new Phrase("c", f)));
            section.AddCell(new PdfPCell(new Phrase("=", f)));
            section.AddCell(new PdfPCell(new Phrase(c.ToString(), f)));
            section.AddCell(new PdfPCell(new Phrase("mm", f)));
            section.AddCell(new PdfPCell(new Phrase(" ", f)));

            report1.Add(section);


            report1.Add(space);

            Paragraph strainingAction = new Paragraph("Straining action", subItemFont);
            report1.Add(strainingAction);

            report1.Add(space);


            PdfPTable Straining = new PdfPTable(6);

            Straining.HorizontalAlignment = 0;
            Straining.DefaultCell.FixedHeight = 20f;
            Straining.TotalWidth = 525f;
            Straining.LockedWidth = true;
            Straining.SetWidths(widths1);
            Straining.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

            Straining.AddCell(new PdfPCell(new Phrase("Ultimate Shear", f)));
            Straining.AddCell(new PdfPCell(new Phrase("Vu", f)));
            Straining.AddCell(new PdfPCell(new Phrase("=", f)));
            Straining.AddCell(new PdfPCell(new Phrase(Vu.ToString(), f)));
            Straining.AddCell(new PdfPCell(new Phrase("kN", f)));
            Straining.AddCell(new PdfPCell(new Phrase(" ", f)));

            report1.Add(Straining);



            //Design Calculations
            Paragraph calcTitle = new Paragraph("Section design calculations", itemFont);
            report1.Add(calcTitle);
            //report.Add(space);


            //------------------------------ Section status -------------------------------------

            
            if (Vu <= Vcn)
            {

                Paragraph SecDesStatus = new Paragraph("Concrete nominal shear resistence >= applied shear stress, use minimum shear reinforcement", f2);
                report1.Add(SecDesStatus);

                report1.Add(space);

                PdfPTable single = new PdfPTable(6);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                single.AddCell(new PdfPCell(new Phrase("Section width", f)));
                single.AddCell(new PdfPCell(new Phrase("b", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(B.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));


                single.AddCell(new PdfPCell(new Phrase("Section depth", f)));
                single.AddCell(new PdfPCell(new Phrase("d", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(d.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d=h-c]", f)));
                
                single.AddCell(new PdfPCell(new Phrase("Strength reduction factor", f)));
                single.AddCell(new PdfPCell(new Phrase("Φ", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(phiShear.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase("[Shear]", f)));

                single.AddCell(new PdfPCell(new Phrase("Modification factor", f)));
                single.AddCell(new PdfPCell(new Phrase("λ", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("1.00", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase("[Normal weight concrete]", f)));

                single.AddCell(new PdfPCell(new Phrase("Concrete nominal shear resistence", f)));
                single.AddCell(new PdfPCell(new Phrase("ΦVc", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Vcn.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("kN", f)));
                single.AddCell(new PdfPCell(new Phrase("[ΦVc = 0.17 λ √(f'c) * b * d]", f)));


                double Avprov = stirBranchNo * (pi * stirBarDia * stirBarDia / 4);
                double Avmin1 = 0.062 * Math.Sqrt(fc) * B * SPmaxround / fy;
                double Avmin2 = 0.35 * B * SPmaxround / fy;
                double Avmin = Math.Max(Avmin1, Avmin2);
                double Av = Math.Max(Avprov, Avmin);

                single.AddCell(new PdfPCell(new Phrase("Minimum area of shear Rft.", f)));
                single.AddCell(new PdfPCell(new Phrase("Avmin", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Av.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[Av(min) = maximum of:]                    [Asminl = 0.062* √(f'c) * b * S /fy]    [Asmin2 = 0.35 * b * S / fy]", f)));

                report1.Add(single);

                report1.Add(space);

                //double Vs = ((Vu - Vcn) / phiShear) * 1000;
                
                double S =Math.Round( Av/(stirBranchNo * pi * stirBarDia * stirBarDia / 4 ),0,MidpointRounding.AwayFromZero);

                double Sprov =Math.Round((S / 25),0,MidpointRounding.AwayFromZero) * 25;

                Paragraph secOutput1 = new Paragraph("Concrete section:    " + B + "mm * " + H + "mm", f3);
                report1.Add(secOutput1);

                Paragraph secOutput2 = new Paragraph("Stiruups              :    " + stirBranchNo + " branches Ø " + stirBarDia + " / " + SPmaxround, f3);
                report1.Add(secOutput2);


            }
            else if (Vu > Vcn && Vu <= Vsecmax)
            {

                Paragraph SecDesStatus = new Paragraph("Shear reinforcement required", f2);
                report1.Add(SecDesStatus);

                report1.Add(space);

                PdfPTable single = new PdfPTable(6);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                single.AddCell(new PdfPCell(new Phrase("Section width", f)));
                single.AddCell(new PdfPCell(new Phrase("b", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(B.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));

                single.AddCell(new PdfPCell(new Phrase("Section depth", f)));
                single.AddCell(new PdfPCell(new Phrase("d", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(d.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d=h-c]", f)));
                
                single.AddCell(new PdfPCell(new Phrase("Strength reduction factor", f)));
                single.AddCell(new PdfPCell(new Phrase("Φ", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(phiShear.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase("[Shear]", f)));

                single.AddCell(new PdfPCell(new Phrase("Modification factor", f)));
                single.AddCell(new PdfPCell(new Phrase("λ", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("1.00", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase("[Normal weight concrete]", f)));

                double nomVc = Math.Round(Vcn, 0);

                single.AddCell(new PdfPCell(new Phrase("Concrete nominal shear resistence", f)));
                single.AddCell(new PdfPCell(new Phrase("ΦVc", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(nomVc.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("kN", f)));
                single.AddCell(new PdfPCell(new Phrase("[ΦVc = Φ * 0.17 λ √(f'c) * b * d]", f)));

                double nomVsmax = Math.Round(Vsmaxn, 0);

                single.AddCell(new PdfPCell(new Phrase("Maximum nominal shear resistence by Rft.", f)));
                single.AddCell(new PdfPCell(new Phrase("ΦVs,max", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(nomVsmax.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("kN", f)));
                single.AddCell(new PdfPCell(new Phrase("[ΦVs,max = Φ * 0.66 √(f'c) * b * d]", f)));

                double Vs = Vu - Vcn;
                double Vsn = ((Vu - Vcn) / phiShear) * 1000;
                double Avprov = stirBranchNo * (pi * stirBarDia * stirBarDia / 4);
                double Avmin1 = 0.062 * Math.Sqrt(fc) * B * SPmaxround / fy;
                double Avmin2 = 0.35 * B * SPmaxround / fy;
                double Avmin = Math.Max(Avmin1, Avmin2);
                double Av = Math.Max(Avprov, Avmin);
                double S = Math.Ceiling(Av * fy * d / Vsn);
                double Sprov = (Math.Round(S / 25)) * 25;
                double AvpS = Vsn / (fy * d);

                single.AddCell(new PdfPCell(new Phrase("Nominal Shear force sustained by Rft.", f)));
                single.AddCell(new PdfPCell(new Phrase("ΦVs", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Vsn.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("kN", f)));
                single.AddCell(new PdfPCell(new Phrase("[ΦVs = Vu - ΦVc]", f)));

                

                single.AddCell(new PdfPCell(new Phrase("Required shear Rft.", f)));
                single.AddCell(new PdfPCell(new Phrase("Av/S", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(AvpS.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2/mm", f)));
                single.AddCell(new PdfPCell(new Phrase("Av/S = Vs / (fy * d)", f)));

                single.AddCell(new PdfPCell(new Phrase("Provided Rft. area for shear per stirrup ", f)));
                single.AddCell(new PdfPCell(new Phrase("Av(prov)", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Avprov.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm2", f)));
                single.AddCell(new PdfPCell(new Phrase("[(π * Φ2 / 4) * number of branches]", f)));

                double stSpacing = Math.Round(S, 0);

                single.AddCell(new PdfPCell(new Phrase("Stirrup spacing ", f)));
                single.AddCell(new PdfPCell(new Phrase("S", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(S.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[(Av/s) /Av]", f)));

                report1.Add(single);

                Paragraph secOutput1 = new Paragraph("Concrete section:    " + B + "mm * " + H + "mm", f3);
                report1.Add(secOutput1);

                Paragraph secOutput2 = new Paragraph("Stiruups              :    " + stirBranchNo + " branches Ø " + stirBarDia + " / " + Sprov, f3);
                report1.Add(secOutput2);


            }

              else
            {

                report1.Add(space);

                PdfPTable single = new PdfPTable(6);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


                report1.Add(space);

                single.HorizontalAlignment = 0;
                single.DefaultCell.FixedHeight = 20f;
                single.TotalWidth = 525f;
                single.LockedWidth = true;
                single.SetWidths(widths1);
                single.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                single.AddCell(new PdfPCell(new Phrase("Section depth", f)));
                single.AddCell(new PdfPCell(new Phrase("d", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(d.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("mm", f)));
                single.AddCell(new PdfPCell(new Phrase("[d=h-c]", f)));

                single.AddCell(new PdfPCell(new Phrase("Strength reduction factor", f)));
                single.AddCell(new PdfPCell(new Phrase("Φ", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(phiShear.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase("[Shear]", f)));

                single.AddCell(new PdfPCell(new Phrase("Modification factor", f)));
                single.AddCell(new PdfPCell(new Phrase("λ", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase("1.00", f)));
                single.AddCell(new PdfPCell(new Phrase(" ", f)));
                single.AddCell(new PdfPCell(new Phrase("[Normal weight concrete]", f)));

                single.AddCell(new PdfPCell(new Phrase("Concrete nominal shear resistence", f)));
                single.AddCell(new PdfPCell(new Phrase("ΦVc", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(Vcn.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("kN", f)));
                single.AddCell(new PdfPCell(new Phrase("[ΦVc = 0.17 λ √(f'c) * b * d]", f)));

                double maxVs = Math.Round(Vsmaxn);

                single.AddCell(new PdfPCell(new Phrase("Maximum Shear resistence", f)));
                single.AddCell(new PdfPCell(new Phrase("ΦVsmax", f)));
                single.AddCell(new PdfPCell(new Phrase("=", f)));
                single.AddCell(new PdfPCell(new Phrase(maxVs.ToString(), f)));
                single.AddCell(new PdfPCell(new Phrase("kN", f)));
                single.AddCell(new PdfPCell(new Phrase("[ΦVs < Vu]", f)));

                report1.Add(single);

                Paragraph secOutput1 = new Paragraph("ΦVs < Vu", f4);
                report1.Add(secOutput1);

                Paragraph secOutput2 = new Paragraph("The section is unsafe shear, increase section", f4);
                report1.Add(secOutput2);

                //return;
            }

            report1.Close();

            MessageBox.Show("PDF report created on Desktop", "BeaD 1.1", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
        }

        private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("https://mr-khaled-fayed.wixsite.com/mysite");
        }
    }


        }

