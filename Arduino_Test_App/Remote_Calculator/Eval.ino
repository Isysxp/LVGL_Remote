#include <stdio.h>
#include <string.h>
#include <math.h>

// Function declarations
float addsub();
float muldiv();
float term();

char input[101]; // Declare character array to store user input
int pos = 0;    // Initialize a global variable to keep track of the position in the input string

// Function to parse and evaluate a term (number or expression inside parentheses)
float term(){
  float n = 0;

  // Check if the current character is an opening parenthesis
  if(input[pos] == '('){
    pos++; // Move to the next character
    n = addsub(); // Evaluate the expression inside the parentheses

    // Check if the next character is a closing parenthesis
    if(input[pos] == ')'){
      pos++; // Move to the next character (closing parenthesis)
      return n; // Return the evaluated result
    }
  }else{
    // If the current character is a digit, parse the number. See %n description in sscanf
    int ctr;
    sscanf(&input[pos],"%f%n",&n,&ctr);
    pos+=ctr;
  }
  return n; // Return the parsed number
}

// Function to handle multiplication and division operations
float muldiv(){
  float first,second;

  first = term(); // Evaluate the first term

  for(;;){
    if(input[pos] == '*'){
      pos++; // Move to the next character (operator)
      second = term(); // Evaluate the second term
      first *= second; // Perform multiplication
    }else if(input[pos] == '/'){
      pos++; // Move to the next character (operator)
      second = term(); // Evaluate the second term
      first /= second; // Perform division
    }else{
      return first; // If no more operators, return the result
    }
  }
}

// Function to handle addition and subtraction operations
float addsub(){
  float first,second;
  pos = 0;
  first = muldiv(); // Evaluate the first term

  for(;;){
    if(input[pos] == '+'){
      pos++; // Move to the next character (operator)
      second = muldiv(); // Evaluate the second term
      first += second; // Perform addition
    }else if(input[pos] == '-'){
      pos++; // Move to the next character (operator)
      second = muldiv(); // Evaluate the second term
      first -= second; // Perform subtraction
    }else{
      return first; // If no more operators, return the result
    }
  }
}

