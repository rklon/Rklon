using System;

namespace SalarityCalc
{
	abstract class Umowa
	{
		protected ulong brutto, netto;
		protected ulong przychod, dochod;
		protected ulong zusEmertytalna, zusRentowa, zusChorobowa;
		//protected ulong zusWypadkowa, zusFPiFS;
		protected ulong zusSuma;
		protected ulong nfzZdrowotna;
		protected ulong pitZaliczka;

		public Umowa(ulong val)
		//	dodać konstruktor z double, bo będzie potrzebny przy formularzu
		{
			brutto = val*100;
			przychod = brutto;
		}

		protected ulong getBrutto()
		{
			return brutto;
		}

		//	testowanie funkcji Round():
		//	domyślnie źle zaokrągla 0.5; drugi argument działa na część ułamkową
		//	testowanie zaokrąglania przez gównojadów z ms:
		//	poprawić na tabelkę z trzeba metodami
		public void roundTest()
		{
			for (dochod = 0; dochod < 12; dochod++)
				Console.WriteLine(dochod + " " + Math.Round(dochod*.1, MidpointRounding.AwayFromZero));
		}

		protected ulong obliczProcent(ulong val, double procent)
		{
			/*double t1 = procent * value / 100;
			double t2 = Math.Round(t1);
			uint t3 = (uint)t2;

			Console.WriteLine("double: " + t1);
			Console.WriteLine("round : " + t2);
			Console.WriteLine("uint  : " + t3);
			Console.WriteLine("norm  : " + t3/100.0);

			return t3;*/

			// !uwaga na zaokrąglenia; domyślnie mikromiękki robi je oczywiście źle!
			return (ulong)Math.Round(val * procent / 100, MidpointRounding.AwayFromZero);	//	wynik '/100' bo mnożę przez procenty (%)
		}

		protected abstract ulong policzZUS();

		protected virtual ulong policzNFZ()
		{
			return nfzZdrowotna = obliczProcent(dochod, 9);		//	9,00%	9,00%
		}

		protected abstract ulong policzPIT();

		public void obliczNetto()
		{
			dochod = przychod - policzZUS();
			netto  = dochod   - policzNFZ() - policzPIT();
		}

		//	mam netto, a szukam brutto:
		public void obliczBrutto()
		{
			ulong a, b, i = 0,
				  v, m;		//	v - value, m - medium

			v = brutto;		//	zapamiętanie obliczanej wartości netto
			a = v;			//	określenie zakresu poszukiwań
			b = 2 * a;
			Console.WriteLine(i++ + ": " + a + " " + b);

			do {
				m = (a + b) / 2;
				brutto = przychod = m;
				obliczNetto();
				if (v > netto) a = m;
				else		   b = m;
				Console.WriteLine(i++ + ": " + a + " " + b);
			} while (v != netto);

			Console.WriteLine(m);
			Console.WriteLine(v);
		}

		public void wypisz()
		{
			Console.WriteLine("Brutto     : " + brutto			/ 100.0);
			Console.WriteLine("Składki ZUS: ");
			Console.WriteLine("-emerytalna: " + zusEmertytalna	/ 100.0);
			Console.WriteLine("-rentowa   : " + zusRentowa		/ 100.0);
			//Console.WriteLine("-wypadkowy : " + zusWypadkowa	/ 100.0);
			Console.WriteLine("-chorobowy : " + zusChorobowa	/ 100.0);
			//Console.WriteLine("-FP i FS   : " + zusFPiFS		/ 100.0);
			Console.WriteLine("= razem ZUS: " + zusSuma			/ 100.0);
			Console.WriteLine("Dochód     : " + dochod			/ 100.0);
			Console.WriteLine("Zdrowotna  : " + nfzZdrowotna	/ 100.0);
			Console.WriteLine("ZaliczkaPIT: " + pitZaliczka		/ 100.0);
			Console.WriteLine("Netto      : " + netto			/ 100.0);
		}
	}

	class UmowaOPracę : Umowa
	{
		public UmowaOPracę(ulong val) : base(val) {}

		protected override ulong policzZUS()
		{
			//	Minimalna podstawa, od której liczy się składki w 2023 roku, wynosi 4161 zł
			zusEmertytalna	= obliczProcent(przychod, 9.76);		//	19,52%	9,76%
			zusRentowa		= obliczProcent(przychod, 1.50);		//	 8,00%	1,50%
			zusChorobowa	= obliczProcent(przychod, 2.45);		//	 2,45%	2,45%; całą płaci pracownik
			//zusWypadkowa	= obliczProcent(przychod, 1.67);		//	 zmienna: od 0,67% do 3,33%; 1.67%, ale płaci pracodawca
			//zusFPiFS		= obliczProcent(przychod, 2.45);		//	 2,45%, ale płaci pracodawca

			//return zusSuma = zusEmertytalna + zusRentowa + zusWypadkowa + zusChorobowa + zusFPiFS;
			return zusSuma = zusEmertytalna + zusRentowa + zusChorobowa;
		}

		protected override ulong policzPIT()
		{
			ulong kwotaWolna = 30000 * 100;		//	od 2022r.; '*100' bo wartość w groszach
			ulong kwotaZmniejszającaPodatek = obliczProcent(kwotaWolna, 12) / 12;	//	12.00%; /12 miesięcy = 300zł
			//Console.WriteLine(kwotaZmniejszającaPodatek);

			ulong koszty = 250 * 100;			//	albo 300; generalnie jest do tego gdzieś jakaś tabela 2x2

			//dochod = 2500 * 100 + 50;
			//ulong podstawaOpodatkowania = dochod - nfzZdrowotna;	//	ni kuta! nie zgadza się zaliczka!

			//	!! zaokrąglenie do pełnych złotych - nie usuwać !!
			//	!!! tu jest błąd; jeśli dochód < koszty wynik jest niepoprawny; ściślej występuje przepełnienie, a że typ jest unsigned to otrzymujemy b.dużą wartość dodatnią
			ulong podstawaOpodatkowania = 0;
			if (dochod > koszty)
				podstawaOpodatkowania = (ulong)Math.Round((dochod-koszty) / 100.0, MidpointRounding.AwayFromZero) * 100;
			//Console.WriteLine(podstawaOpodatkowania);

			//Console.WriteLine(obliczProcent(podstawaOpodatkowania, 12));
			pitZaliczka = obliczProcent(podstawaOpodatkowania, 12);		//	12%
			if (pitZaliczka > kwotaZmniejszającaPodatek)
			{
				pitZaliczka -= kwotaZmniejszającaPodatek;
				//	!! zaokrąglenie do pełnych złotych - nie usuwać !!
				return pitZaliczka = (ulong)Math.Round(pitZaliczka / 100.0, MidpointRounding.AwayFromZero) * 100;
			}
			return pitZaliczka = 0;
		}
	}

	class UmowaZlecenie : Umowa
	{
		public UmowaZlecenie(ulong val) : base(val) {}

		protected override ulong policzZUS()
		{
			zusEmertytalna	= obliczProcent(przychod, 9.76);
			zusRentowa		= obliczProcent(przychod, 1.50);

			return zusSuma = zusEmertytalna + zusRentowa;
		}

		protected override ulong policzPIT()
		{
			ulong kwotaWolna = 30000 * 100;		//	od 2022r.; '*100' bo wartość w groszach
			ulong kwotaZmniejszającaPodatek = obliczProcent(kwotaWolna, 12) / 12;  //	12.00%; /12 miesięcy = 300zł

			ulong koszty = obliczProcent(dochod, 20);	//	albo 20% albo 50% dochodu

			ulong podstawaOpodatkowania = (ulong)Math.Round((dochod - koszty) / 100.0, MidpointRounding.AwayFromZero) * 100;
			//Console.WriteLine(podstawaOpodatkowania);

			//Console.WriteLine(obliczProcent(podstawaOpodatkowania, 12));
			pitZaliczka = obliczProcent(podstawaOpodatkowania, 12);		//	12%
			if (pitZaliczka > kwotaZmniejszającaPodatek)
			{
				pitZaliczka -= kwotaZmniejszającaPodatek;
				return pitZaliczka = (ulong)Math.Round(pitZaliczka / 100.0, MidpointRounding.AwayFromZero) * 100;
			}
			return pitZaliczka = 0;
		}
	}

	class UmowaODzieło : Umowa
	{
		public UmowaODzieło(ulong val) : base(val) {}

		protected override ulong policzZUS()
		{
			return 0;
		}

		protected override ulong policzNFZ()
		{
			return 0;
		}

		protected override ulong policzPIT()
		{
			//	Czyżby nie była uwzględniana kwota wolna od opodatkowania??

			ulong koszty = obliczProcent(dochod, 20);	//	albo 20% albo 50% dochodu

			ulong podstawaOpodatkowania = (ulong)Math.Round((dochod - koszty) / 100.0, MidpointRounding.AwayFromZero) * 100;

			pitZaliczka = obliczProcent(podstawaOpodatkowania, 12);	//	12%

			return pitZaliczka = (ulong)Math.Round(pitZaliczka / 100.0, MidpointRounding.AwayFromZero) * 100;
		}
	}

	internal class Calculator
	{
		static void Main(string[] args) {
			Umowa umowa;

			umowa = new UmowaOPracę(10000);
			//umowa = new UmowaOPracę(3200);		//	dla pitZaliczka = 1,00 zł
			//umowa = new UmowaOPracę(290);			//	dla podstawyOpodatkowania = 0,00 zł

			//umowa = new UmowaZlecenie(10000);
			//umowa = new UmowaODzieło(10000);

			umowa.obliczNetto();
			umowa.wypisz();

			Console.WriteLine();

			//	obliczenia kwoty brutto z netto:
			umowa = new UmowaOPracę(5000);
			umowa.obliczBrutto();
			umowa.wypisz();
		}
	}
}

//	+ umowa o pracę
//	+ umowa zlecenie
//	+ umowa o dzieło
//	+ obliczenia netto -> brutto
//	+ WEB interface

//	?
//	https://zus.pox.pl/
//	https://wynagrodzenia.pl/kalkulator-wynagrodzen