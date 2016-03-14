hirom
header

!dropLvlA = $40
!dropLvlB = $40
!dropLvlC = $40
!dropTable = $CF3002

!dropLvlAB = $80
!droplvlABC = $C0

org $C25F1E
JSL newDrop
BRA continue

org $C25F31
continue:

org $F20000
newDrop:
REP #$30
LDA $2001,Y
ASL
ASL
ASL
PHA
SEP #$30
LDA $1F6D      ; RNG index
INC $1F6D
TAX            ; Transfer A to X
LDA $C0FD00,X  ; Load a random number
REP #$10
PLX
CMP #!dropLvlA
BCC dropA
CMP #!dropLvlAB
BCC dropB
CMP #!droplvlABC
BCC dropC
INX
dropC:
INX
dropB:
INX
dropA:
LDA !dropTable,X
RTL

steal:	
ASL	   ; Temp
TAX            ;(enemy number * 8) 
LDA $CF3000,X
STA $3308,Y    ;(enemy steal slots)
RTL

org $C22C3D
ASL 
ASL 
PHA
JSL steal
PLA
TAX
BRA continueSteal

org $C22C49
continueSteal:

