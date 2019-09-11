using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTest")]

namespace SubstitutionCipher
{
	class Program
	{
		static void Main(string[] args)
		{
			int c = 3, b = 4;
			Console.WriteLine("Hello World!");
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
			c = c + b;

			Calendar.Caland();
		}
	}
	
	internal class Calendar
	{
		public static void Caland()
		{
			DateTime now = GetCurrentDate();
			Console.WriteLine($"\nToday's date is {now}\n");
			Console.ReadKey();
		}
		internal static DateTime GetCurrentDate()
		{
			return DateTime.Now.Date;
		}
	}
}


//https://docs.microsoft.com/ko-kr/visualstudio/liveshare/
//svm = static void main, 코드 조각 사용: 입력 - 탭 탭
// ctrl+k ctrl+c = 주석
// ctrl+k ctrl+u = 주석 제거
// ctrl+m ctrl+m = 코드 블럭 축소/확장
// alt+F12 = 정의 피킹(peeking)
// ctrl+r ctrl+r 변수명 한번에 수정하고 엔터 - 이름 리팩터링