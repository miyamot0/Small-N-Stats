namespace Small_N_Stats.Model
{
    class TAUU
    {
        public string Name { get; set; }
        public string Range { get; set; }
        public bool IsChecked { get; set; }
        public bool IsCorrected { get; set; }

        /*
        Holders for UI tags 
        */

        public bool Reflective { get; set; }
        public double S { get; set; }
        public double Pairs { get; set; }
        public double Ties { get; set; }
        public double TAU { get; set; }
        public double TAUB { get; set; }
        public double VARs { get; set; }
        public double SD { get; set; }
        public double SDtau { get; set; }
        public double Z { get; set; }
        public double PValue { get; set; }
        public double[] CI_85 { get; set; }
        public double[] CI_90 { get; set; }
        public double[] CI_95 { get; set; }
    }
}
