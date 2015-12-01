// ConsoleApplication1.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include <deque>
#include <iostream>

int _tmain(int argc, _TCHAR* argv[])
{
   using namespace std;
   deque <int> c1;
   deque <int>::iterator c1_Iter;

   c1.push_back( 10 );
   c1.push_back( 20 );
   c1.push_back( 30 );
   c1.push_back( 40 );

   for ( c1_Iter = c1.begin( ); c1_Iter != c1.end( ); c1_Iter++ )
   {
	   if(*c1_Iter == 20)
		   *c1_Iter = 200;
   }
      
}

