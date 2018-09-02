PUBLIC asm_hook_connect;
PUBLIC asm_hook_wsaconnect;

_DATA SEGMENT
_DATA ENDS

_TEXT SEGMENT

EXTERN hook_connect: PROC

EXTERN connectTramp: qword
EXTERN WSAConnectTramp: qword

EXTERN send_sock_patch: PROC

EXTERN sockBackup: qword
EXTERN sockAddrNew: qword
EXTERN sockAddrBackup: qword
EXTERN retAddr: qword


asm_hook_connect PROC
	mov sockAddrBackup, rdx;
	push rax;
	push r9;
	push r10;
	push r11;
	pushfq;
	push r8;

	
	
	push rdx;
	push rcx;

	call hook_connect;

	pop rcx;
	add rsp, 8;
	mov rdx, sockAddrNew;

	pop r8;
	popfq;
	pop r11;
	pop r10;
	pop r9;
	pop rax;
	
	pop retAddr;
	call connectTramp;
	push retAddr;

	push rcx;
	push rdx;
	push r8;
	push r9;
	push r10;
	pushfq;

	mov rcx, sockBackup;
	sub rsp, 8;
	call send_sock_patch;
	add rsp, 8;

	popfq;
	pop r10;
	pop r9;
	pop r8;
	pop rdx;
	pop rcx;

	mov rax, 0;

	ret;

asm_hook_connect ENDP


asm_hook_wsaconnect PROC
	mov sockAddrBackup, rdx;
	push rbx;
	push rax;
	push r9;
	push r10;
	push r11;
	pushfq;
	push r8;

	
	
	push rdx;
	push rcx;

	call hook_connect;

	pop rcx;
	add rsp, 8;
	mov rdx, sockAddrNew;

	pop r8;
	popfq;
	pop r11;
	pop r10;
	pop r9;
	pop rax;
	pop rbx;
	
	pop retAddr;
	call WSAConnectTramp;
	push retAddr;

	push rcx;
	push rdx;
	push r8;
	push r9;
	push r10;
	pushfq;

	mov rcx, sockBackup;
	sub rsp, 8;
	call send_sock_patch;
	add rsp, 8;

	popfq;
	pop r10;
	pop r9;
	pop r8;
	pop rdx;
	pop rcx;

	mov rax, 0;

	ret;

asm_hook_wsaconnect ENDP

_TEXT ENDS
END