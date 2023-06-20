# redline-deobfuscator

##redline-deobfuscator.exe redlineSample.exe  deobfuscatedRedLineSample.exe
Intr

- This executable/code is trying to deobfuscate the redline strings obfuscations, to make the life more easier.

## Usage

```
redline-deobfuscator.exe redlineSample.exe  deobfuscatedRedLineSample.exe
```


## How it works ?

- The deobfuscator work with the below two types of obfuscation:

  ### Deobfuscation of string made of char array
 
  - Redline is obfuscating the strings by constructing them using big array of char to make it harder for reading as below

     ```
      new string(new char[]{
				'E',
				'x',
				't',
				'e',
				'n',
				's',
				'i',
				'o',
				'n',
				' ',
				'C',
				'o',
				'o',
				'k',
				'i',
				'e',
				's'
			  })
      ```

  - The below is the equivalent IL code
  
      ```
      2	0006	ldc.i4.s	17
      3	0008	newarr	 [mscorlib]System.Char
      4	000D	dup
      5	000E	ldtoken	 valuetype dnlibDotNetPdbDssSymbolNamespaceImplb/'__StaticArrayInitTypeSize=34' dnlibDotNetPdbDssSymbolNamespaceImplb::'99086C63443EF4224B60D2ED08447C082E7A0484'
      6	0013	call	 void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array, valuetype [mscorlib]System.RuntimeFieldHandle)
      7	0018	newobj	 instance void [mscorlib]System.String::.ctor(char[])
     ```


  -  After the execution of deobfuscator, the code will look like the below.
    
     ```
       "Extension Cookies"
     ```
  
  - and the equivalent IL code will be like that
   
    ```
    2	0006	nop
    3	0007	nop
    4	0008	nop
    5	0009	nop
    6	000A	nop
    7	000B	ldstr	"Extension Cookies"
    ```





  ### Deobfuscation of strings that use replace functio
  - hello
  
## Dependencies
- This deobfuscator is built by depending on [**dnlib**](https://github.com/0xd4d/dnlib) library. 
 
