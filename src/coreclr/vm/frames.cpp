// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#include "common.h"
#include "log.h"
#include "frames.h"
#include "threads.h"
#include "object.h"
#include "method.hpp"
#include "class.h"
#include "excep.h"
#include "stublink.h"
#include "fieldmarshaler.h"
#include "siginfo.hpp"
#include "gcheaputilities.h"
#include "dllimportcallback.h"
#include "stackwalk.h"
#include "dbginterface.h"
#include "eeconfig.h"
#include "ecall.h"
#include "clsload.hpp"
#include "cgensys.h"
#include "virtualcallstub.h"
#include "dllimport.h"
#include "gcrefmap.h"
#include "asmconstants.h"

#ifdef FEATURE_COMINTEROP
#include "comtoclrcall.h"
#endif // FEATURE_COMINTEROP

#include "argdestination.h"

#ifdef FEATURE_INTERPRETER
#include "interpexec.h"
#endif // FEATURE_INTERPRETER

#define CHECK_APP_DOMAIN    0

#ifdef DACCESS_COMPILE
#define FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE() _ASSERTE("!Unexpected value in Frame")
#else
#define FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE() DoJITFailFast()
#endif

void Frame::GcScanRoots(promote_func *fn, ScanContext* sc)
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GcScanRoots_Impl(fn, sc); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return;
    }
}

unsigned Frame::GetFrameAttribs()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetFrameAttribs_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return 0;
    }
}

#ifndef DACCESS_COMPILE
void Frame::ExceptionUnwind()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->ExceptionUnwind_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return;
    }
}
#endif

BOOL Frame::NeedsUpdateRegDisplay()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->NeedsUpdateRegDisplay_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return FALSE;
    }
}

BOOL Frame::IsTransitionToNativeFrame()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->IsTransitionToNativeFrame_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return FALSE;
    }
}

MethodDesc *Frame::GetFunction()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetFunction_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return NULL;
    }
}

Assembly *Frame::GetAssembly()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetAssembly_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return NULL;
    }
}

PTR_BYTE Frame::GetIP()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetIP_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return NULL;
    }
}

TADDR Frame::GetReturnAddressPtr()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetReturnAddressPtr_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return (TADDR)0;
    }
}

PCODE Frame::GetReturnAddress()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetReturnAddress_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return (PCODE)NULL;
    }
}

void Frame::UpdateRegDisplay(const PREGDISPLAY pRegDisplay, bool updateFloats)
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->UpdateRegDisplay_Impl(pRegDisplay, updateFloats); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return;
    }
}

int Frame::GetFrameType()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetFrameType_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return 0;
    }
}

Frame::ETransitionType Frame::GetTransitionType()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetTransitionType_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return (ETransitionType)0;
    }
}

Frame::Interception Frame::GetInterception()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetInterception_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return (Interception)0;
    }
}

void Frame::GetUnmanagedCallSite(TADDR* ip, TADDR* returnIP, TADDR* returnSP)
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetUnmanagedCallSite_Impl(ip, returnIP, returnSP); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return;
    }
}

BOOL Frame::TraceFrame(Thread *thread, BOOL fromPatch, TraceDestination *trace, REGDISPLAY *regs)
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->TraceFrame_Impl(thread, fromPatch, trace, regs); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return FALSE;
    }
}

#ifdef DACCESS_COMPILE
void Frame::EnumMemoryRegions(CLRDataEnumMemoryFlags flags)
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->EnumMemoryRegions_Impl(flags); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return;
    }
}
#endif // DACCESS_COMPILE

#if defined(_DEBUG) && !defined(DACCESS_COMPILE)
BOOL Frame::Protects(OBJECTREF *ppObjectRef)
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->Protects_Impl(ppObjectRef); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return FALSE;
    }
}
#endif // defined(_DEBUG) && !defined(DACCESS_COMPILE)

// TransitionFrame only apis
TADDR TransitionFrame::GetTransitionBlock()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->GetTransitionBlock_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return (TADDR)0;
    }
}

BOOL TransitionFrame::SuppressParamTypeArg()
{
    switch (GetFrameIdentifier())
    {
#define FRAME_TYPE_NAME(frameType) case FrameIdentifier::frameType: { return dac_cast<PTR_##frameType>(this)->SuppressParamTypeArg_Impl(); }
#include "FrameTypes.h"
    default:
        FRAME_POLYMORPHIC_DISPATCH_UNREACHABLE();
        return FALSE;
    }
}

//-----------------------------------------------------------------------
#if _DEBUG
//-----------------------------------------------------------------------

#ifndef DACCESS_COMPILE

unsigned dbgStubCtr = 0;
unsigned dbgStubTrip = 0xFFFFFFFF;

void Frame::Log() {
    WRAPPER_NO_CONTRACT;

    if (!LoggingOn(LF_STUBS, LL_INFO1000000))
        return;

    dbgStubCtr++;
    if (dbgStubCtr > dbgStubTrip) {
        dbgStubCtr++;      // basicly a nop to put a breakpoint on.
    }

    MethodDesc* method = GetFunction();

    STRESS_LOG3(LF_STUBS, LL_INFO1000000, "STUBS: In Stub with Frame %p assoc Method %pM FrameType = %pV\n", this, method, *((void**) this));

    char buff[64];
    const char* frameType;
    if (GetFrameIdentifier() == FrameIdentifier::PrestubMethodFrame)
        frameType = "PreStub";
    else if (GetFrameIdentifier() == FrameIdentifier::PInvokeCalliFrame)
    {
        sprintf_s(buff, ARRAY_SIZE(buff), "PInvoke CALLI target" FMT_ADDR,
                  DBG_ADDR(((PInvokeCalliFrame*)this)->GetPInvokeCalliTarget()));
        frameType = buff;
    }
    else if (GetFrameIdentifier() == FrameIdentifier::StubDispatchFrame)
        frameType = "StubDispatch";
    else if (GetFrameIdentifier() == FrameIdentifier::ExternalMethodFrame)
        frameType = "ExternalMethod";
    else
        frameType = "Unknown";

    if (method != 0)
        LOG((LF_STUBS, LL_INFO1000000,
             "IN %s Stub Method = %s::%s SIG %s ESP of return" FMT_ADDR "\n",
             frameType,
             method->m_pszDebugClassName,
             method->m_pszDebugMethodName,
             method->m_pszDebugMethodSignature,
             DBG_ADDR(GetReturnAddressPtr())));
    else
        LOG((LF_STUBS, LL_INFO1000000,
             "IN %s Stub Method UNKNOWN ESP of return" FMT_ADDR "\n",
             frameType,
             DBG_ADDR(GetReturnAddressPtr()) ));

    _ASSERTE(GetThread()->PreemptiveGCDisabled());
}

//-----------------------------------------------------------------------
// This function is used to log transitions in either direction
// between unmanaged code and CLR/managed code.
// This is typically done in a stub that sets up a Frame, which is
// passed as an argument to this function.

void __stdcall Frame::LogTransition(Frame* frame)
{

    CONTRACTL {
        DEBUG_ONLY;
        NOTHROW;
        ENTRY_POINT;
        GC_NOTRIGGER;
    } CONTRACTL_END;

    if (Frame::ShouldLogTransitions())
        frame->Log();
} // void Frame::Log()

#endif // #ifndef DACCESS_COMPILE

//-----------------------------------------------------------------------
#endif // _DEBUG
//-----------------------------------------------------------------------


// TODO [DAVBR]: For the full fix for VsWhidbey 450273, all the below
// may be uncommented once isLegalManagedCodeCaller works properly
// with non-return address inputs, and with non-DEBUG builds
#if 0
//-----------------------------------------------------------------------
// returns TRUE if retAddr, is a return address that can call managed code

bool isLegalManagedCodeCaller(PCODE retAddr) {
    WRAPPER_NO_CONTRACT;
#ifdef TARGET_X86

        // we expect to be called from JITTED code or from special code sites inside
        // mscorwks like callDescr which we have put a NOP (0x90) so we know that they
        // are specially blessed.
    if (!ExecutionManager::IsManagedCode(retAddr) &&
        (
#ifdef DACCESS_COMPILE
         !(PTR_BYTE(retAddr).IsValid()) ||
#endif
         ((*PTR_BYTE(retAddr) != 0x90) &&
          (*PTR_BYTE(retAddr) != 0xcc))))
    {
        LOG((LF_GC, LL_INFO10, "Bad caller to managed code: retAddr=0x%08x, *retAddr=0x%x\n",
             retAddr, *(BYTE*)PTR_BYTE(retAddr)));

        return false;
    }

        // it better be a return address of some kind
    TADDR dummy;
    if (isRetAddr(retAddr, &dummy))
        return true;

#ifndef DACCESS_COMPILE
#ifdef DEBUGGING_SUPPORTED
    // The debugger could have dropped an INT3 on the instruction that made the call
    // Calls can be 2 to 7 bytes long
    if (CORDebuggerAttached()) {
        PTR_BYTE ptr = PTR_BYTE(retAddr);
        for (int i = -2; i >= -7; --i)
            if (ptr[i] == 0xCC)
                return true;
        return false;
    }
#endif // DEBUGGING_SUPPORTED
#endif // #ifndef DACCESS_COMPILE

    _ASSERTE(!"Bad return address on stack");
    return false;
#else  // TARGET_X86
    return true;
#endif // TARGET_X86
}
#endif //0


//-----------------------------------------------------------------------
// Implementation of the global table of names
#define FRAME_TYPE_NAME(x) #x,
static const LPCSTR FrameTypeNameTable[] = {
#include "frames.h"
};


/* static */
LPCSTR Frame::GetFrameTypeName(FrameIdentifier frameIdentifier)
{
    LIMITED_METHOD_CONTRACT;
    if ((frameIdentifier == FrameIdentifier::None) || frameIdentifier >= FrameIdentifier::CountPlusOne)
    {
        return NULL;
    }
    return FrameTypeNameTable[(int)frameIdentifier - 1];
} // char* Frame::FrameTypeName()


#if defined (_DEBUG_IMPL)   // _DEBUG and !DAC

//-----------------------------------------------------------------------


void Frame::LogFrame(
    int         LF,                     // Log facility for this call.
    int         LL)                     // Log Level for this call.
{
    char        buf[32];
    const char  *pFrameType;

    pFrameType = GetFrameTypeName(GetFrameIdentifier());

    if (pFrameType == NULL)
    {
        _ASSERTE(!"New Frame type needs to be added to FrameTypeName()");
        // Pointer is up to 17chars + vtbl@ = 22 chars
        sprintf_s(buf, ARRAY_SIZE(buf), "frameIdentifier@%p", (VOID *)GetFrameIdentifier());
        pFrameType = buf;
    }

    LOG((LF, LL, "FRAME: addr:%p, next:%p, type:%s\n",
         this, m_Next, pFrameType));
} // void Frame::LogFrame()

void Frame::LogFrameChain(
    int         LF,                     // Log facility for this call.
    int         LL)                     // Log Level for this call.
{
    if (!LoggingOn(LF, LL))
        return;

    Frame *pFrame = this;
    while (pFrame != FRAME_TOP)
    {
        pFrame->LogFrame(LF, LL);
        pFrame = pFrame->m_Next;
    }
} // void Frame::LogFrameChain()

//-----------------------------------------------------------------------
#endif // _DEBUG_IMPL
//-----------------------------------------------------------------------

#ifndef DACCESS_COMPILE

void Frame::Init(FrameIdentifier frameIdentifier)
{
    LIMITED_METHOD_CONTRACT;
    _frameIdentifier = frameIdentifier;
} // void Frame::Init()

#endif // DACCESS_COMPILE

// Returns true if the Frame has a valid FrameIdentifier

// static
bool Frame::HasValidFrameIdentifier(Frame * pFrame)
{
    WRAPPER_NO_CONTRACT;

    if (pFrame == NULL || pFrame == FRAME_TOP)
        return false;

    FrameIdentifier vptr = pFrame->GetFrameIdentifier();
    //
    // Use a simple compare which is dependent on the tightly packed arrangement of FrameIdentifier
    //
    return (((TADDR)vptr > (TADDR)FrameIdentifier::None) && ((TADDR)vptr < (TADDR)FrameIdentifier::CountPlusOne));
}

//-----------------------------------------------------------------------
#ifndef DACCESS_COMPILE
//-----------------------------------------------------------------------
// Link and Unlink this frame.
//-----------------------------------------------------------------------

VOID Frame::Push()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    Push(GetThread());
}

VOID Frame::Push(Thread *pThread)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    m_Next = pThread->GetFrame();

    // GetOsPageSize() is used to relax the assert for cases where two Frames are
    // declared in the same source function. We cannot predict the order
    // in which the C compiler will lay them out in the stack frame.
    // So GetOsPageSize() is a guess of the maximum stack frame size of any method
    // with multiple Frames in coreclr.dll
    _ASSERTE((pThread->IsExecutingOnAltStack() ||
             (m_Next == FRAME_TOP) ||
             (PBYTE(m_Next) + (2 * GetOsPageSize())) > PBYTE(this)) &&
             "Pushing a frame out of order ?");

    _ASSERTE(// If AssertOnFailFast is set, the test expects to do stack overrun
             // corruptions. In that case, the Frame chain may be corrupted,
             // and the rest of the assert is not valid.
             // Note that the corrupted Frame chain will be detected
             // during stack-walking.
             !g_pConfig->fAssertOnFailFast() ||
             (m_Next == FRAME_TOP) ||
             (Frame::HasValidFrameIdentifier(m_Next)));

    pThread->SetFrame(this);
}

VOID Frame::Pop()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    Pop(GetThread());
}

VOID Frame::Pop(Thread *pThread)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    _ASSERTE(pThread->GetFrame() == this && "Popping a frame out of order ?");
    _ASSERTE(// If AssertOnFailFast is set, the test expects to do stack overrun
             // corruptions. In that case, the Frame chain may be corrupted,
             // and the rest of the assert is not valid.
             // Note that the corrupted Frame chain will be detected
             // during stack-walking.
             !g_pConfig->fAssertOnFailFast() ||
             (m_Next == FRAME_TOP) ||
             (Frame::HasValidFrameIdentifier(m_Next)));

    pThread->SetFrame(m_Next);
    m_Next = NULL;
}

#if defined(TARGET_UNIX) && !defined(DACCESS_COMPILE)
void Frame::PopIfChained()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    if (m_Next != NULL)
    {
        GCX_COOP();
        // When the frame is destroyed, make sure it is no longer in the
        // frame chain managed by the Thread.
        Pop();
    }
}
#endif // TARGET_UNIX && !DACCESS_COMPILE

#if !defined(TARGET_X86) || defined(TARGET_UNIX)
/* static */
void Frame::UpdateFloatingPointRegisters(const PREGDISPLAY pRD)
{
    _ASSERTE(!ExecutionManager::IsManagedCode(::GetIP(pRD->pCurrentContext)));
    while (!ExecutionManager::IsManagedCode(::GetIP(pRD->pCurrentContext)))
    {
#ifdef TARGET_UNIX
        PAL_VirtualUnwind(pRD->pCurrentContext, NULL);
#else
        Thread::VirtualUnwindCallFrame(pRD);
#endif
    }
}
#endif // !TARGET_X86 || TARGET_UNIX

//-----------------------------------------------------------------------
#endif // #ifndef DACCESS_COMPILE
//---------------------------------------------------------------
// Get the extra param for shared generic code.
//---------------------------------------------------------------
PTR_VOID TransitionFrame::GetParamTypeArg()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        SUPPORTS_DAC;
    }
    CONTRACTL_END;

    // This gets called while creating stack traces during exception handling.
    // Using the ArgIterator constructor calls ArgIterator::Init which calls GetInitialOfsAdjust
    // which calls SizeOfArgStack, which thinks it may load value types.
    // However all these will have previously been loaded.
    //
    // I'm not entirely convinced this is the best places to put this: CrawlFrame::GetExactGenericArgsToken
    // may be another option.
    ENABLE_FORBID_GC_LOADER_USE_IN_THIS_SCOPE();

    MethodDesc *pFunction = GetFunction();
    _ASSERTE (pFunction->RequiresInstArg());

    MetaSig msig(pFunction);
    ArgIterator argit (&msig);

    INT offs = argit.GetParamTypeArgOffset();

    TADDR taParamTypeArg = *PTR_TADDR(GetTransitionBlock() + offs);
    return PTR_VOID(taParamTypeArg);
}

TADDR TransitionFrame::GetAddrOfThis()
{
    WRAPPER_NO_CONTRACT;
    return GetTransitionBlock() + ArgIterator::GetThisOffset();
}

VASigCookie * TransitionFrame::GetVASigCookie()
{
#if defined(TARGET_X86)
    LIMITED_METHOD_CONTRACT;
    return dac_cast<PTR_VASigCookie>(
        *dac_cast<PTR_TADDR>(GetTransitionBlock() +
        sizeof(TransitionBlock)));
#else
    WRAPPER_NO_CONTRACT;
    MetaSig msig(GetFunction());
    ArgIterator argit(&msig);
    return PTR_VASigCookie(
        *dac_cast<PTR_TADDR>(GetTransitionBlock() + argit.GetVASigCookieOffset()));
#endif
}

#ifndef DACCESS_COMPILE
PrestubMethodFrame::PrestubMethodFrame(TransitionBlock * pTransitionBlock, MethodDesc * pMD)
    : FramedMethodFrame(FrameIdentifier::PrestubMethodFrame, pTransitionBlock, pMD)
{
    LIMITED_METHOD_CONTRACT;
}
#endif // #ifndef DACCESS_COMPILE

BOOL PrestubMethodFrame::TraceFrame_Impl(Thread *thread, BOOL fromPatch,
                                    TraceDestination *trace, REGDISPLAY *regs)
{
    WRAPPER_NO_CONTRACT;

    //
    // We want to set a frame patch, unless we're already at the
    // frame patch, in which case we'll trace the method entrypoint.
    //

    if (fromPatch)
    {
        // In between the time where the Prestub read the method entry point from the slot and the time it reached
        // ThePrestubPatchLabel, GetMethodEntryPoint() could have been updated due to code versioning. This will result in the
        // debugger getting some version of the code or the prestub, but not necessarily the exact code pointer that winds up
        // getting executed. The debugger has code that handles this ambiguity by placing a breakpoint at the start of all
        // native code versions, even if they aren't the one that was reported by this trace, see
        // DebuggerController::PatchTrace() under case TRACE_MANAGED. This alleviates the StubManager from having to prevent the
        // race that occurs here.
        trace->InitForStub(GetFunction()->GetMethodEntryPointIfExists());
    }
    else
    {
        trace->InitForStub(GetPreStubEntryPoint());
    }

    LOG((LF_CORDB, LL_INFO10000,
         "PrestubMethodFrame::TraceFrame: ip=" FMT_ADDR "\n", DBG_ADDR(trace->GetAddress()) ));

    return TRUE;
}

#ifndef DACCESS_COMPILE
//-----------------------------------------------------------------------
// A rather specialized routine for the exclusive use of StubDispatch.
//-----------------------------------------------------------------------
StubDispatchFrame::StubDispatchFrame(TransitionBlock * pTransitionBlock)
    : FramedMethodFrame(FrameIdentifier::StubDispatchFrame, pTransitionBlock, NULL)
{
    LIMITED_METHOD_CONTRACT;

    m_pRepresentativeMT = NULL;
    m_representativeSlot = 0;

    m_pZapModule = NULL;
    m_pIndirection = (TADDR)NULL;

    m_pGCRefMap = NULL;
}

#endif // #ifndef DACCESS_COMPILE

MethodDesc* StubDispatchFrame::GetFunction_Impl()
{
    CONTRACTL {
        NOTHROW;
        GC_NOTRIGGER;
    } CONTRACTL_END;

    MethodDesc * pMD = m_pMD;

    if (m_pMD == NULL)
    {
        if (m_pRepresentativeMT != NULL)
        {
            pMD = m_pRepresentativeMT->GetMethodDescForSlot_NoThrow(m_representativeSlot);
#ifndef DACCESS_COMPILE
            m_pMD = pMD;
#endif
        }
    }

    return pMD;
}

static PTR_BYTE FindGCRefMap(PTR_Module pZapModule, TADDR ptr)
{
    LIMITED_METHOD_DAC_CONTRACT;

    PEImageLayout *pNativeImage = pZapModule->GetReadyToRunImage();

    RVA rva = pNativeImage->GetDataRva(ptr);

    PTR_READYTORUN_IMPORT_SECTION pImportSection = pZapModule->GetImportSectionForRVA(rva);
    if (pImportSection == NULL)
        return NULL;

    COUNT_T index = (rva - pImportSection->Section.VirtualAddress) / pImportSection->EntrySize;

    PTR_BYTE pGCRefMap = dac_cast<PTR_BYTE>(pNativeImage->GetRvaData(pImportSection->AuxiliaryData));
    _ASSERTE(pGCRefMap != NULL);

    // GCRefMap starts with lookup index to limit size of linear scan that follows.
    PTR_BYTE p = pGCRefMap + dac_cast<PTR_DWORD>(pGCRefMap)[index / GCREFMAP_LOOKUP_STRIDE];
    COUNT_T remaining = index % GCREFMAP_LOOKUP_STRIDE;

    while (remaining > 0)
    {
        while ((*p & 0x80) != 0)
            p++;
        p++;

        remaining--;
    }

    return p;
}

PTR_BYTE StubDispatchFrame::GetGCRefMap()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
    }
    CONTRACTL_END;

    PTR_BYTE pGCRefMap = m_pGCRefMap;

    if (pGCRefMap == NULL)
    {
        if (m_pIndirection != (TADDR)NULL)
        {
            if (m_pZapModule == NULL)
            {
                m_pZapModule = ExecutionManager::FindModuleForGCRefMap(m_pIndirection);
            }

            if (m_pZapModule != NULL)
            {
                pGCRefMap = FindGCRefMap(m_pZapModule, m_pIndirection);
            }

#ifndef DACCESS_COMPILE
            if (pGCRefMap != NULL)
            {
                m_pGCRefMap = pGCRefMap;
            }
            else
            {
                // Clear the indirection to avoid retrying
                m_pIndirection = (TADDR)NULL;
            }
#endif
        }
    }

    return pGCRefMap;
}

void StubDispatchFrame::GcScanRoots_Impl(promote_func *fn, ScanContext* sc)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
    }
    CONTRACTL_END

    FramedMethodFrame::GcScanRoots_Impl(fn, sc);

    PTR_BYTE pGCRefMap = GetGCRefMap();
    if (pGCRefMap != NULL)
    {
        PromoteCallerStackUsingGCRefMap(fn, sc, pGCRefMap);
    }
    else
    {
        PromoteCallerStack(fn, sc);
    }
}

BOOL StubDispatchFrame::TraceFrame_Impl(Thread *thread, BOOL fromPatch,
                                    TraceDestination *trace, REGDISPLAY *regs)
{
    WRAPPER_NO_CONTRACT;

    // StubDispatchFixupWorker and VSD_ResolveWorker never directly call managed code. Returning false instructs the debugger to
    // step out of the call that erected this frame and continuing trying to trace execution from there.
    LOG((LF_CORDB, LL_INFO1000, "StubDispatchFrame::TraceFrame: return FALSE\n"));

    return FALSE;
}

Frame::Interception StubDispatchFrame::GetInterception_Impl()
{
    LIMITED_METHOD_CONTRACT;

    return INTERCEPTION_NONE;
}

#ifndef DACCESS_COMPILE
CallCountingHelperFrame::CallCountingHelperFrame(TransitionBlock *pTransitionBlock, MethodDesc *pMD)
    : FramedMethodFrame(FrameIdentifier::CallCountingHelperFrame, pTransitionBlock, pMD)
{
    WRAPPER_NO_CONTRACT;
}
#endif

void CallCountingHelperFrame::GcScanRoots_Impl(promote_func *fn, ScanContext *sc)
{
    WRAPPER_NO_CONTRACT;

    FramedMethodFrame::GcScanRoots_Impl(fn, sc);
    PromoteCallerStack(fn, sc);
}

BOOL CallCountingHelperFrame::TraceFrame_Impl(Thread *thread, BOOL fromPatch, TraceDestination *trace, REGDISPLAY *regs)
{
    WRAPPER_NO_CONTRACT;

    // OnCallCountThresholdReached never directly calls managed code. Returning false instructs the debugger to step out of the
    // call that erected this frame and continuing trying to trace execution from there.
    LOG((LF_CORDB, LL_INFO1000, "CallCountingHelperFrame::TraceFrame: return FALSE\n"));
    return FALSE;
}

#ifndef DACCESS_COMPILE
ExternalMethodFrame::ExternalMethodFrame(TransitionBlock * pTransitionBlock)
    : FramedMethodFrame(FrameIdentifier::ExternalMethodFrame, pTransitionBlock, NULL)
{
    LIMITED_METHOD_CONTRACT;

    m_pIndirection = (TADDR)NULL;
    m_pZapModule = NULL;

    m_pGCRefMap = NULL;
}
#endif // !DACCESS_COMPILE

void ExternalMethodFrame::GcScanRoots_Impl(promote_func *fn, ScanContext* sc)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
    }
    CONTRACTL_END

    FramedMethodFrame::GcScanRoots_Impl(fn, sc);
    PromoteCallerStackUsingGCRefMap(fn, sc, GetGCRefMap());
}

PTR_BYTE ExternalMethodFrame::GetGCRefMap()
{
    LIMITED_METHOD_DAC_CONTRACT;

    PTR_BYTE pGCRefMap = m_pGCRefMap;

    if (pGCRefMap == NULL)
    {
        if (m_pIndirection != (TADDR)NULL)
        {
            pGCRefMap = FindGCRefMap(m_pZapModule, m_pIndirection);
#ifndef DACCESS_COMPILE
            m_pGCRefMap = pGCRefMap;
#endif
        }
    }

    _ASSERTE(pGCRefMap != NULL);
    return pGCRefMap;
}

Frame::Interception ExternalMethodFrame::GetInterception_Impl()
{
    LIMITED_METHOD_CONTRACT;

    return INTERCEPTION_NONE;
}

Frame::Interception PrestubMethodFrame::GetInterception_Impl()
{
    LIMITED_METHOD_DAC_CONTRACT;

    //
    // The only direct kind of interception done by the prestub
    // is class initialization.
    //

    return INTERCEPTION_PRESTUB;
}

#ifndef DACCESS_COMPILE
DynamicHelperFrame::DynamicHelperFrame(TransitionBlock * pTransitionBlock, int dynamicHelperFrameFlags)
    : FramedMethodFrame(FrameIdentifier::DynamicHelperFrame, pTransitionBlock, NULL)
{
    LIMITED_METHOD_CONTRACT;

    m_dynamicHelperFrameFlags = dynamicHelperFrameFlags;
}
#endif // !DACCESS_COMPILE

void DynamicHelperFrame::GcScanRoots_Impl(promote_func *fn, ScanContext* sc)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
    }
    CONTRACTL_END

    FramedMethodFrame::GcScanRoots_Impl(fn, sc);

    PTR_PTR_Object pArgumentRegisters = dac_cast<PTR_PTR_Object>(GetTransitionBlock() + TransitionBlock::GetOffsetOfArgumentRegisters());

    if (m_dynamicHelperFrameFlags & DynamicHelperFrameFlags_ObjectArg)
    {
        TADDR pArgument = GetTransitionBlock() + TransitionBlock::GetOffsetOfArgumentRegisters();
#ifdef TARGET_X86
        // x86 is special as always
        pArgument += offsetof(ArgumentRegisters, ECX);
#endif
        (*fn)(dac_cast<PTR_PTR_Object>(pArgument), sc, CHECK_APP_DOMAIN);
    }

    if (m_dynamicHelperFrameFlags & DynamicHelperFrameFlags_ObjectArg2)
    {
        TADDR pArgument = GetTransitionBlock() + TransitionBlock::GetOffsetOfArgumentRegisters();
#ifdef TARGET_X86
        // x86 is special as always
        pArgument += offsetof(ArgumentRegisters, EDX);
#else
        pArgument += sizeof(TADDR);
#endif
        (*fn)(dac_cast<PTR_PTR_Object>(pArgument), sc, CHECK_APP_DOMAIN);
    }
}

#ifndef DACCESS_COMPILE

#ifdef FEATURE_COMINTEROP
//-----------------------------------------------------------------------
// A rather specialized routine for the exclusive use of the COM PreStub.
//-----------------------------------------------------------------------
VOID
ComPrestubMethodFrame::Init()
{
    WRAPPER_NO_CONTRACT;

    // Initializes the frame's identifier.
    Frame::Init(FrameIdentifier::ComPrestubMethodFrame);
}
#endif // FEATURE_COMINTEROP

//-----------------------------------------------------------------------
// GCFrames
//-----------------------------------------------------------------------


//--------------------------------------------------------------------
// This constructor pushes a new GCFrame on the frame chain.
//--------------------------------------------------------------------
GCFrame::GCFrame(Thread *pThread, OBJECTREF *pObjRefs, UINT numObjRefs, BOOL maybeInterior)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(pThread != NULL);
    }
    CONTRACTL_END;

#ifdef USE_CHECKED_OBJECTREFS
    if (!maybeInterior) {
        UINT i;
        for(i = 0; i < numObjRefs; i++)
            Thread::ObjectRefProtected(&pObjRefs[i]);

        for (i = 0; i < numObjRefs; i++) {
            pObjRefs[i].Validate();
        }
    }

#if 0  // We'll want to restore this goodness check at some time. For now, the fact that we use
       // this as temporary backstops in our loader exception conversions means we're highly
       // exposed to infinite stack recursion should the loader be invoked during a stackwalk.
       // So we'll do without.

    if (g_pConfig->GetGCStressLevel() != 0 && IsProtectedByGCFrame(pObjRefs)) {
        _ASSERTE(!"This objectref is already protected by a GCFrame. Protecting it twice will corrupt the GC.");
    }
#endif

#endif // USE_CHECKED_OBJECTREFS

#ifdef _DEBUG
    m_Next          = NULL;
    m_pCurThread    = NULL;
#endif // _DEBUG

    m_pObjRefs      = pObjRefs;
    m_numObjRefs    = numObjRefs;
    m_MaybeInterior = maybeInterior;

    Push(pThread);
}

GCFrame::~GCFrame()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(m_pCurThread != NULL);
    }
    CONTRACTL_END;

    // m_pNext is NULL when the frame was already popped from the stack.
    if (m_Next != NULL)
    {
        // This is a GCFrame that was not popped.  This is a problem.
        // We should have popped it before we destruct
        // Do a manual switch to the GC cooperative mode instead of using the GCX_COOP_THREAD_EXISTS
        // macro so that this function isn't slowed down by having to deal with FS:0 chain on x86 Windows.
        BOOL wasCoop = m_pCurThread->PreemptiveGCDisabled();
        if (!wasCoop)
        {
            m_pCurThread->DisablePreemptiveGC();
        }

        Pop();

        if (!wasCoop)
        {
            m_pCurThread->EnablePreemptiveGC();
        }
    }
}

void GCFrame::Push(Thread* pThread)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(pThread != NULL);
        PRECONDITION(m_Next == NULL);
        PRECONDITION(m_pCurThread == NULL);
    }
    CONTRACTL_END;

    // Push the GC frame to the per-thread list
    m_Next = pThread->GetGCFrame();
    m_pCurThread = pThread;

    // GetOsPageSize() is used to relax the assert for cases where two Frames are
    // declared in the same source function. We cannot predict the order
    // in which the compiler will lay them out in the stack frame.
    // So GetOsPageSize() is a guess of the maximum stack frame size of any method
    // with multiple GCFrames in coreclr.dll
    _ASSERTE(((m_Next == GCFRAME_TOP) ||
              (PBYTE(m_Next) + (2 * GetOsPageSize())) > PBYTE(this)) &&
             "Pushing a GCFrame out of order ?");

    pThread->SetGCFrame(this);
}

void GCFrame::Pop()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(m_pCurThread != NULL);
    }
    CONTRACTL_END;

    // When the frame is destroyed, make sure it is no longer in the
    // frame chain managed by the Thread.
    // It also cancels the GC protection provided by the frame.

    _ASSERTE(m_pCurThread->GetGCFrame() == this && "Popping a GCFrame out of order ?");

    m_pCurThread->SetGCFrame(m_Next);
    m_Next = NULL;

#ifdef _DEBUG
    m_pCurThread->EnableStressHeap();
    for(UINT i = 0; i < m_numObjRefs; i++)
        Thread::ObjectRefNew(&m_pObjRefs[i]);       // Unprotect them
#endif
}

void GCFrame::Remove()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(m_pCurThread != NULL);
    }
    CONTRACTL_END;

    GCFrame *pPrevFrame = NULL;
    GCFrame *pFrame = m_pCurThread->GetGCFrame();
    while (pFrame != GCFRAME_TOP)
    {
        if (pFrame == this)
        {
            if (pPrevFrame)
            {
                pPrevFrame->m_Next = m_Next;
            }
            else
            {
                m_pCurThread->SetGCFrame(m_Next);
            }

            m_Next = NULL;

#ifdef _DEBUG
            m_pCurThread->EnableStressHeap();
            for(UINT i = 0; i < m_numObjRefs; i++)
                Thread::ObjectRefNew(&m_pObjRefs[i]);       // Unprotect them
#endif
            break;
        }

        pPrevFrame = pFrame;
        pFrame = pFrame->m_Next;
    }

    _ASSERTE_MSG(pFrame != NULL, "GCFrame not found in the current thread's stack");
}

#endif // !DACCESS_COMPILE

//
// GCFrame Object Scanning
//
// This handles scanning/promotion of GC objects that were
// protected by the programmer explicitly protecting it in a GC Frame
// via the GCPROTECTBEGIN / GCPROTECTEND facility...
//
void GCFrame::GcScanRoots(promote_func *fn, ScanContext* sc)
{
    WRAPPER_NO_CONTRACT;

    PTR_PTR_Object pRefs = dac_cast<PTR_PTR_Object>(m_pObjRefs);

    for (UINT i = 0; i < m_numObjRefs; i++)
    {
        auto fromAddress = OBJECTREF_TO_UNCHECKED_OBJECTREF(m_pObjRefs[i]);
        if (m_MaybeInterior)
        {
            PromoteCarefully(fn, pRefs + i, sc, GC_CALL_INTERIOR | CHECK_APP_DOMAIN);
        }
        else
        {
            (*fn)(pRefs + i, sc, 0);
        }

        auto toAddress = OBJECTREF_TO_UNCHECKED_OBJECTREF(m_pObjRefs[i]);
        LOG((LF_GC, INFO3, "GC Protection Frame promoted" FMT_ADDR "to" FMT_ADDR "\n",
            DBG_ADDR(fromAddress), DBG_ADDR(toAddress)));
    }
}


#ifndef DACCESS_COMPILE

#if defined(_DEBUG) && !defined (DACCESS_COMPILE)

struct IsProtectedByGCFrameStruct
{
    OBJECTREF       *ppObjectRef;
    UINT             count;
};

static StackWalkAction IsProtectedByGCFrameStackWalkFramesCallback(
    CrawlFrame      *pCF,
    VOID            *pData
)
{
    DEBUG_ONLY_FUNCTION;
    WRAPPER_NO_CONTRACT;

    IsProtectedByGCFrameStruct *pd = (IsProtectedByGCFrameStruct*)pData;
    Frame *pFrame = pCF->GetFrame();
    if (pFrame) {
        if (pFrame->Protects(pd->ppObjectRef)) {
            pd->count++;
        }
    }
    return SWA_CONTINUE;
}

BOOL IsProtectedByGCFrame(OBJECTREF *ppObjectRef)
{
    DEBUG_ONLY_FUNCTION;
    WRAPPER_NO_CONTRACT;

    // Just report TRUE if GCStress is not on.  This satisfies the asserts that use this
    // code without the cost of actually determining it.
    if (!GCStress<cfg_any>::IsEnabled())
        return TRUE;

    if (ppObjectRef == NULL) {
        return TRUE;
    }

    CONTRACT_VIOLATION(ThrowsViolation);
    ENABLE_FORBID_GC_LOADER_USE_IN_THIS_SCOPE ();
    IsProtectedByGCFrameStruct d = {ppObjectRef, 0};
    GetThread()->StackWalkFrames(IsProtectedByGCFrameStackWalkFramesCallback, &d);

    GCFrame* pGCFrame = GetThread()->GetGCFrame();
    while (pGCFrame != GCFRAME_TOP)
    {
        if (pGCFrame->Protects(ppObjectRef)) {
            d.count++;
        }

        pGCFrame = pGCFrame->PtrNextFrame();
    }

    if (d.count > 1) {
        _ASSERTE(!"Multiple GCFrames protecting the same pointer. This will cause GC corruption!");
    }
    return d.count != 0;
}
#endif // _DEBUG

#endif //!DACCESS_COMPILE

#ifdef FEATURE_HIJACK
#ifdef TARGET_X86
void HijackFrame::GcScanRoots_Impl(promote_func *fn, ScanContext* sc)
{
    LIMITED_METHOD_CONTRACT;

    bool hasAsyncRet;
    ReturnKind returnKind = m_Thread->GetHijackReturnKind(&hasAsyncRet);
    _ASSERTE(IsValidReturnKind(returnKind));

    int regNo = 0;
    bool moreRegisters = false;

    do
    {
        ReturnKind r = ExtractRegReturnKind(returnKind, regNo, moreRegisters);
        PTR_PTR_Object objPtr = dac_cast<PTR_PTR_Object>(&m_Args->ReturnValue[regNo]);

        switch (r)
        {
        case RT_Float: // Fall through
        case RT_Scalar:
            // nothing to report
            break;

        case RT_Object:
            LOG((LF_GC, INFO3, "Hijack Frame Promoting Object" FMT_ADDR "to",
                DBG_ADDR(OBJECTREF_TO_UNCHECKED_OBJECTREF(*objPtr))));
            (*fn)(objPtr, sc, CHECK_APP_DOMAIN);
            LOG((LF_GC, INFO3, FMT_ADDR "\n", DBG_ADDR(OBJECTREF_TO_UNCHECKED_OBJECTREF(*objPtr))));
            break;

        case RT_ByRef:
            LOG((LF_GC, INFO3, "Hijack Frame Carefully Promoting pointer" FMT_ADDR "to",
                DBG_ADDR(OBJECTREF_TO_UNCHECKED_OBJECTREF(*objPtr))));
            PromoteCarefully(fn, objPtr, sc, GC_CALL_INTERIOR);
            LOG((LF_GC, INFO3, FMT_ADDR "\n", DBG_ADDR(OBJECTREF_TO_UNCHECKED_OBJECTREF(*objPtr))));
            break;

        default:
            _ASSERTE(!"Impossible two bit encoding");
        }

        regNo++;
    } while (moreRegisters);

    if (hasAsyncRet)
    {
        PTR_PTR_Object objPtr = dac_cast<PTR_PTR_Object>(&m_Args->AsyncRet);
        LOG((LF_GC, INFO3, "Hijack Frame Promoting Async Continuation Return" FMT_ADDR "to",
            DBG_ADDR(OBJECTREF_TO_UNCHECKED_OBJECTREF(*objPtr))));
        (*fn)(objPtr, sc, CHECK_APP_DOMAIN);
        LOG((LF_GC, INFO3, FMT_ADDR "\n", DBG_ADDR(OBJECTREF_TO_UNCHECKED_OBJECTREF(*objPtr))));
    }
}
#endif // TARGET_X86
#endif // FEATURE_HIJACK

void ProtectValueClassFrame::GcScanRoots_Impl(promote_func *fn, ScanContext *sc)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
    }
    CONTRACTL_END

    ValueClassInfo *pVCInfo = m_pVCInfo;
    while (pVCInfo != NULL)
    {
        _ASSERTE(pVCInfo->pMT->IsValueType());
        ReportPointersFromValueType(fn, sc, pVCInfo->pMT, pVCInfo->pData);
        pVCInfo = pVCInfo->pNext;
    }
}

//
// Promote Caller Stack
//
//

void TransitionFrame::PromoteCallerStack(promote_func* fn, ScanContext* sc)
{
    WRAPPER_NO_CONTRACT;

    // I believe this is the contract:
    //CONTRACTL
    //{
    //    INSTANCE_CHECK;
    //    NOTHROW;
    //    GC_NOTRIGGER;
    //    FORBID_FAULT;
    //    MODE_ANY;
    //}
    //CONTRACTL_END

    MethodDesc *pFunction;

    LOG((LF_GC, INFO3, "    Promoting method caller Arguments\n" ));

    // We're going to have to look at the signature to determine
    // which arguments a are pointers....First we need the function
    pFunction = GetFunction();
    if (pFunction == NULL)
        return;

    // Now get the signature...
    Signature callSignature = pFunction->GetSignature();
    if (callSignature.IsEmpty())
    {
        return;
    }

    //If not "vararg" calling convention, assume "default" calling convention
    if (!MetaSig::IsVarArg(callSignature))
    {
        SigTypeContext typeContext(pFunction);
        PCCOR_SIGNATURE pSig;
        DWORD cbSigSize;
        pFunction->GetSig(&pSig, &cbSigSize);

        MetaSig msig(pSig, cbSigSize, pFunction->GetModule(), &typeContext);

        bool fCtorOfVariableSizedObject = msig.HasThis() && (pFunction->GetMethodTable() == g_pStringClass) && pFunction->IsCtor();
        if (fCtorOfVariableSizedObject)
            msig.ClearHasThis();

        if (pFunction->RequiresInstArg() && !SuppressParamTypeArg())
            msig.SetHasParamTypeArg();

        if (pFunction->IsAsyncMethod())
            msig.SetIsAsyncCall();

        PromoteCallerStackHelper (fn, sc, pFunction, &msig);
    }
    else
    {
        VASigCookie *varArgSig = GetVASigCookie();

        SigTypeContext typeContext(varArgSig->classInst, varArgSig->methodInst);
        MetaSig msig(varArgSig->signature,
                     varArgSig->pModule,
                     &typeContext);
        PromoteCallerStackHelper (fn, sc, pFunction, &msig);
    }
}

void TransitionFrame::PromoteCallerStackHelper(promote_func* fn, ScanContext* sc,
                                                 MethodDesc *pFunction, MetaSig *pmsig)
{
    WRAPPER_NO_CONTRACT;
    // I believe this is the contract:
    //CONTRACTL
    //{
    //    INSTANCE_CHECK;
    //    NOTHROW;
    //    GC_NOTRIGGER;
    //    FORBID_FAULT;
    //    MODE_ANY;
    //}
    //CONTRACTL_END

    ArgIterator argit(pmsig);

    TADDR pTransitionBlock = GetTransitionBlock();

    // promote 'this' for non-static methods
    if (argit.HasThis() && pFunction != NULL)
    {
        BOOL interior = pFunction->GetMethodTable()->IsValueType() && !pFunction->IsUnboxingStub();

        PTR_PTR_VOID pThis = dac_cast<PTR_PTR_VOID>(pTransitionBlock + argit.GetThisOffset());
        LOG((LF_GC, INFO3,
             "    'this' Argument at " FMT_ADDR "promoted from" FMT_ADDR "\n",
             DBG_ADDR(pThis), DBG_ADDR(*pThis) ));

        if (interior)
            PromoteCarefully(fn, PTR_PTR_Object(pThis), sc, GC_CALL_INTERIOR|CHECK_APP_DOMAIN);
        else
            (fn)(PTR_PTR_Object(pThis), sc, CHECK_APP_DOMAIN);
    }

    int argOffset;
    while ((argOffset = argit.GetNextOffset()) != TransitionBlock::InvalidOffset)
    {
        ArgDestination argDest(dac_cast<PTR_VOID>(pTransitionBlock), argOffset, argit.GetArgLocDescForStructInRegs());
        pmsig->GcScanRoots(&argDest, fn, sc);
    }
}

#ifdef TARGET_X86
UINT TransitionFrame::CbStackPopUsingGCRefMap(PTR_BYTE pGCRefMap)
{
    LIMITED_METHOD_CONTRACT;

    GCRefMapDecoder decoder(pGCRefMap);
    return decoder.ReadStackPop() * sizeof(TADDR);
}
#endif

static UINT OffsetFromGCRefMapPos(int pos)
{
#ifdef TARGET_X86
    return (pos < NUM_ARGUMENT_REGISTERS) ?
            (TransitionBlock::GetOffsetOfArgumentRegisters() + ARGUMENTREGISTERS_SIZE - (pos + 1) * sizeof(TADDR)) :
            (TransitionBlock::GetOffsetOfArgs() + (pos - NUM_ARGUMENT_REGISTERS) * sizeof(TADDR));
#else
    return TransitionBlock::GetOffsetOfFirstGCRefMapSlot() + pos * sizeof(TADDR);
#endif
}

void TransitionFrame::PromoteCallerStackUsingGCRefMap(promote_func* fn, ScanContext* sc, PTR_BYTE pGCRefMap)
{
    WRAPPER_NO_CONTRACT;

    GCRefMapDecoder decoder(pGCRefMap);

#ifdef TARGET_X86
    // Skip StackPop
    decoder.ReadStackPop();
#endif

    TADDR pTransitionBlock = GetTransitionBlock();

    while (!decoder.AtEnd())
    {
        int pos = decoder.CurrentPos();
        int token = decoder.ReadToken();
        int ofs = OffsetFromGCRefMapPos(pos);

        PTR_TADDR ppObj = dac_cast<PTR_TADDR>(pTransitionBlock + ofs);

        switch (token)
        {
        case GCREFMAP_SKIP:
            break;
        case GCREFMAP_REF:
            fn(dac_cast<PTR_PTR_Object>(ppObj), sc, CHECK_APP_DOMAIN);
            break;
        case GCREFMAP_INTERIOR:
            PromoteCarefully(fn, dac_cast<PTR_PTR_Object>(ppObj), sc, GC_CALL_INTERIOR);
            break;
        case GCREFMAP_METHOD_PARAM:
            if (sc->promotion)
            {
#ifndef DACCESS_COMPILE
                MethodDesc *pMDReal = dac_cast<PTR_MethodDesc>(*ppObj);
                if (pMDReal != NULL)
                    GcReportLoaderAllocator(fn, sc, pMDReal->GetLoaderAllocator());
#endif
            }
            break;
        case GCREFMAP_TYPE_PARAM:
            if (sc->promotion)
            {
#ifndef DACCESS_COMPILE
                MethodTable *pMTReal = dac_cast<PTR_MethodTable>(*ppObj);
                if (pMTReal != NULL)
                    GcReportLoaderAllocator(fn, sc, pMTReal->GetLoaderAllocator());
#endif
            }
            break;
        case GCREFMAP_VASIG_COOKIE:
            {
                VASigCookie *varArgSig = dac_cast<PTR_VASigCookie>(*ppObj);

                SigTypeContext typeContext(varArgSig->classInst, varArgSig->methodInst);
                MetaSig msig(varArgSig->signature,
                                varArgSig->pModule,
                                &typeContext);
                PromoteCallerStackHelper (fn, sc, NULL, &msig);
            }
            break;
        default:
            _ASSERTE(!"Unknown GCREFMAP token");
            break;
        }
    }
}

void PInvokeCalliFrame::PromoteCallerStack(promote_func* fn, ScanContext* sc)
{
    WRAPPER_NO_CONTRACT;

    LOG((LF_GC, INFO3, "    Promoting CALLI caller Arguments\n" ));

    // get the signature
    VASigCookie *varArgSig = GetVASigCookie();
    if (varArgSig->signature.IsEmpty())
    {
        return;
    }

    SigTypeContext typeContext(varArgSig->classInst, varArgSig->methodInst);
    MetaSig msig(varArgSig->signature,
                 varArgSig->pModule,
                 &typeContext);
    PromoteCallerStackHelper(fn, sc, NULL, &msig);
}

#ifndef DACCESS_COMPILE
PInvokeCalliFrame::PInvokeCalliFrame(TransitionBlock * pTransitionBlock, VASigCookie * pVASigCookie, PCODE pUnmanagedTarget)
    : FramedMethodFrame(FrameIdentifier::PInvokeCalliFrame, pTransitionBlock, NULL)
{
    LIMITED_METHOD_CONTRACT;

    m_pVASigCookie = pVASigCookie;
    m_pUnmanagedTarget = pUnmanagedTarget;
}
#endif // #ifndef DACCESS_COMPILE

#ifdef FEATURE_COMINTEROP

#ifndef DACCESS_COMPILE
CLRToCOMMethodFrame::CLRToCOMMethodFrame(TransitionBlock * pTransitionBlock, MethodDesc * pMD)
    : FramedMethodFrame(FrameIdentifier::CLRToCOMMethodFrame, pTransitionBlock, pMD)
{
    LIMITED_METHOD_CONTRACT;
}
#endif // #ifndef DACCESS_COMPILE

void CLRToCOMMethodFrame::GcScanRoots_Impl(promote_func* fn, ScanContext* sc)
{
    WRAPPER_NO_CONTRACT;

    // CLRToCOMMethodFrame is only used in the event call / late bound call code path where we do not have IL stub
    // so we need to promote the arguments and return value manually.

    FramedMethodFrame::GcScanRoots_Impl(fn, sc);
    PromoteCallerStack(fn, sc);

    //
    // Promote the returned object
    //

    MetaSig sig(GetFunction());

    TypeHandle thValueType;
    CorElementType et = sig.GetReturnTypeNormalized(&thValueType);
    if (CorTypeInfo::IsObjRef_NoThrow(et))
    {
        (*fn)(GetReturnObjectPtr(), sc, CHECK_APP_DOMAIN);
    }
    else if (CorTypeInfo::IsByRef_NoThrow(et))
    {
        PromoteCarefully(fn, GetReturnObjectPtr(), sc, GC_CALL_INTERIOR | CHECK_APP_DOMAIN);
    }
    else if (et == ELEMENT_TYPE_VALUETYPE)
    {
        ArgIterator argit(&sig);
        if (!argit.HasRetBuffArg())
        {
#ifdef TARGET_UNIX
#error Non-Windows ABIs must be special cased
#endif
            ReportPointersFromValueType(fn, sc, thValueType.AsMethodTable(), GetReturnObjectPtr());
        }
    }
}
#endif // FEATURE_COMINTEROP

#if defined (_DEBUG) && !defined (DACCESS_COMPILE)
// For IsProtectedByGCFrame, we need to know whether a given object ref is protected
// by a CLRToCOMMethodFrame or a ComMethodFrame. Since GCScanRoots for those frames are
// quite complicated, we don't want to duplicate their logic so we call GCScanRoots with
// IsObjRefProtected (a fake promote function) and an extended ScanContext to do the checking.

struct IsObjRefProtectedScanContext : public ScanContext
{
    OBJECTREF * oref_to_check;
    BOOL        oref_protected;
    IsObjRefProtectedScanContext (OBJECTREF * oref)
    {
        thread_under_crawl = GetThread();
        promotion = TRUE;
        oref_to_check = oref;
        oref_protected = FALSE;
    }
};

void IsObjRefProtected (Object** ppObj, ScanContext* sc, uint32_t)
{
    LIMITED_METHOD_CONTRACT;
    IsObjRefProtectedScanContext * orefProtectedSc = (IsObjRefProtectedScanContext *)sc;
    if (ppObj == (Object **)(orefProtectedSc->oref_to_check))
        orefProtectedSc->oref_protected = TRUE;
}

BOOL TransitionFrame::Protects_Impl(OBJECTREF * ppORef)
{
    WRAPPER_NO_CONTRACT;
    IsObjRefProtectedScanContext sc (ppORef);
    // Set the stack limit for the scan to the SP of the managed frame above the transition frame
    sc.stack_limit = GetSP();
    GcScanRoots (IsObjRefProtected, &sc);
    return sc.oref_protected;
}
#endif //defined (_DEBUG) && !defined (DACCESS_COMPILE)

#ifdef FEATURE_COMINTEROP

#ifdef TARGET_X86
// Return the # of stack bytes pushed by the unmanaged caller.
UINT ComMethodFrame::GetNumCallerStackBytes()
{
    WRAPPER_NO_CONTRACT;
    SUPPORTS_DAC;

    ComCallMethodDesc* pCMD = PTR_ComCallMethodDesc((TADDR)GetDatum());
    _ASSERTE(pCMD != NULL);
    // assumes __stdcall
    // compute the callee pop stack bytes
    return pCMD->GetNumStackBytes();
}
#endif // TARGET_X86

#ifndef DACCESS_COMPILE
void ComMethodFrame::DoSecondPassHandlerCleanup(Frame * pCurFrame)
{
    LIMITED_METHOD_CONTRACT;

    // Find ComMethodFrame

    while ((pCurFrame != FRAME_TOP) &&
           (pCurFrame->GetFrameIdentifier() != FrameIdentifier::ComMethodFrame))
    {
        pCurFrame = pCurFrame->PtrNextFrame();
    }

    if (pCurFrame == FRAME_TOP)
        return;

    ComMethodFrame * pComMethodFrame = (ComMethodFrame *)pCurFrame;

    _ASSERTE(pComMethodFrame != NULL);
    Thread * pThread = GetThread();
    GCX_COOP_THREAD_EXISTS(pThread);
    // Unwind the frames till the entry frame (which was ComMethodFrame)
    pCurFrame = pThread->GetFrame();
    while ((pCurFrame != NULL) && (pCurFrame <= pComMethodFrame))
    {
        pCurFrame->ExceptionUnwind();
        pCurFrame = pCurFrame->PtrNextFrame();
    }

    // At this point, pCurFrame would be the ComMethodFrame's predecessor frame
    // that we need to reset to.
    _ASSERTE((pCurFrame != NULL) && (pComMethodFrame->PtrNextFrame() == pCurFrame));
    pThread->SetFrame(pCurFrame);
}
#endif // !DACCESS_COMPILE

#endif // FEATURE_COMINTEROP

#ifndef DACCESS_COMPILE

VOID InlinedCallFrame::Init()
{
    WRAPPER_NO_CONTRACT;

    Frame::Init(FrameIdentifier::InlinedCallFrame);

    m_Datum = NULL;
    m_pCallSiteSP = NULL;
    m_pCallerReturnAddress = (TADDR)NULL;
}


#ifdef FEATURE_COMINTEROP
void UnmanagedToManagedFrame::ExceptionUnwind_Impl()
{
    WRAPPER_NO_CONTRACT;

    AppDomain::ExceptionUnwind(this);
}
#endif // FEATURE_COMINTEROP

#endif // !DACCESS_COMPILE

#ifdef FEATURE_COMINTEROP
PCODE UnmanagedToManagedFrame::GetReturnAddress_Impl()
{
    WRAPPER_NO_CONTRACT;

    PCODE pRetAddr = Frame::GetReturnAddress_Impl();

    if (InlinedCallFrame::FrameHasActiveCall(m_Next) &&
        pRetAddr == m_Next->GetReturnAddress())
    {
        // there's actually no unmanaged code involved - we were called directly
        // from managed code using an InlinedCallFrame
        return NULL;
    }
    else
    {
        return pRetAddr;
    }
}
#endif // FEATURE_COMINTEROP

#ifdef FEATURE_INTERPRETER

TADDR InterpreterFrame::DummyCallerIP = (TADDR)&InterpreterFrame::DummyFuncletCaller;

PTR_InterpMethodContextFrame InterpreterFrame::GetTopInterpMethodContextFrame()
{
    LIMITED_METHOD_CONTRACT;
    PTR_InterpMethodContextFrame pFrame = m_pTopInterpMethodContextFrame;
    _ASSERTE(pFrame != NULL);

    // The pFrame points to the last known topmost interpreter frame for the related InterpExecMethod.
    // For regular execution, it is always the current topmost one. However, in the case of a dump
    // debugging or a native runtime debugging, it may be pointing to a higher or lower frame and
    // we need to seek to the right one.
    if (pFrame->ip != NULL)
    {
        // The frame is active, so it is either the topmost one or we need to seek towards the top
        // of the stack.
        while ((pFrame->pNext != NULL) && (pFrame->pNext->ip != NULL))
        {
            pFrame = pFrame->pNext;
        }
    }
    else
    {
        // The frame is not active, which means it a frame that was used before, but the interpreter
        // already returned from it and zeroed its ip. The frame is ready for reuse by another call.
        // We need to seek for an active one towards the bottom of the stack.
        // It can also represent a case when the interpreter hasn't started interpreting the method
        // yet, but the frame was already created.
        while (pFrame->pParent != NULL && pFrame->ip == NULL)
        {
            pFrame = pFrame->pParent;
        }
    }

    return pFrame;
}

void InterpreterFrame::SetContextToInterpMethodContextFrame(T_CONTEXT * pContext)
{
    PTR_InterpMethodContextFrame pFrame = GetTopInterpMethodContextFrame();
    SetIP(pContext, (TADDR)pFrame->ip);
    SetSP(pContext, dac_cast<TADDR>(pFrame));
    SetFP(pContext, (TADDR)pFrame->pStack);
    SetFirstArgReg(pContext, dac_cast<TADDR>(this));
    pContext->ContextFlags = CONTEXT_FULL;
    if (m_isFaulting)
    {
        pContext->ContextFlags |= CONTEXT_EXCEPTION_ACTIVE;
    }
}

void InterpreterFrame::UpdateRegDisplay_Impl(const PREGDISPLAY pRD, bool updateFloats)
{
    SyncRegDisplayToCurrentContext(pRD);
    TransitionFrame::UpdateRegDisplay_Impl(pRD, updateFloats);
}

#ifndef DACCESS_COMPILE
void InterpreterFrame::ExceptionUnwind_Impl()
{
    WRAPPER_NO_CONTRACT;

    Thread *pThread = GetThread();
    InterpThreadContext *pThreadContext = pThread->GetInterpThreadContext();
    InterpMethodContextFrame *pInterpMethodContextFrame = m_pTopInterpMethodContextFrame;

    // Unwind the interpreter frames belonging to the current InterpreterFrame.
    while (pInterpMethodContextFrame != NULL)
    {
        pThreadContext->frameDataAllocator.PopInfo(pInterpMethodContextFrame);
        pThreadContext->pStackPointer = pInterpMethodContextFrame->pStack;
        pInterpMethodContextFrame = pInterpMethodContextFrame->pParent;
    }
}
#endif // !DACCESS_COMPILE

#endif // FEATURE_INTERPRETER

#ifndef DACCESS_COMPILE
//=================================================================================

void FakePromote(PTR_PTR_Object ppObj, ScanContext *pSC, uint32_t dwFlags)
{
    CONTRACTL {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    } CONTRACTL_END;

    CORCOMPILE_GCREFMAP_TOKENS newToken = (dwFlags & GC_CALL_INTERIOR) ? GCREFMAP_INTERIOR : GCREFMAP_REF;

    _ASSERTE((*(CORCOMPILE_GCREFMAP_TOKENS *)ppObj == 0) || (*(CORCOMPILE_GCREFMAP_TOKENS *)ppObj == newToken));

    *(CORCOMPILE_GCREFMAP_TOKENS *)ppObj = newToken;
}

//=================================================================================

void FakePromoteCarefully(promote_func *fn, Object **ppObj, ScanContext *pSC, uint32_t dwFlags)
{
    (*fn)(ppObj, pSC, dwFlags);
}

//=================================================================================

void FakeGcScanRoots(MetaSig& msig, ArgIterator& argit, MethodDesc * pMD, BYTE * pFrame)
{
    STANDARD_VM_CONTRACT;

    ScanContext sc;

    // Encode generic instantiation arg
    if (argit.HasParamType())
    {
        // Note that intrinsic array methods have hidden instantiation arg too, but it is not reported to GC
        if (pMD->RequiresInstMethodDescArg())
            *(CORCOMPILE_GCREFMAP_TOKENS *)(pFrame + argit.GetParamTypeArgOffset()) = GCREFMAP_METHOD_PARAM;
        else
        if (pMD->RequiresInstMethodTableArg())
            *(CORCOMPILE_GCREFMAP_TOKENS *)(pFrame + argit.GetParamTypeArgOffset()) = GCREFMAP_TYPE_PARAM;
    }

    // If the function has a this pointer, add it to the mask
    if (argit.HasThis())
    {
        BOOL interior = pMD->GetMethodTable()->IsValueType() && !pMD->IsUnboxingStub();

        FakePromote((Object **)(pFrame + argit.GetThisOffset()), &sc, interior ? GC_CALL_INTERIOR : 0);
    }

    if (argit.IsVarArg())
    {
        *(CORCOMPILE_GCREFMAP_TOKENS *)(pFrame + argit.GetVASigCookieOffset()) = GCREFMAP_VASIG_COOKIE;

        // We are done for varargs - the remaining arguments are reported via vasig cookie
        return;
    }

    //
    // Now iterate the arguments
    //

    // Cycle through the arguments, and call msig.GcScanRoots for each
    int argOffset;
    while ((argOffset = argit.GetNextOffset()) != TransitionBlock::InvalidOffset)
    {
        ArgDestination argDest(pFrame, argOffset, argit.GetArgLocDescForStructInRegs());
        msig.GcScanRoots(&argDest, &FakePromote, &sc, &FakePromoteCarefully);
    }
}

#ifdef _DEBUG
static void DumpGCRefMap(const char *name, BYTE *address)
{
    GCRefMapDecoder decoder(address);

    StackSString buf;

    buf.Printf("%s GC ref map: ", name);
#if TARGET_X86
    uint32_t stackPop = decoder.ReadStackPop();
    buf.AppendPrintf("POP(0x%x)", stackPop);
#endif

    int previousToken = GCREFMAP_SKIP;
    while (!decoder.AtEnd())
    {
        int pos = decoder.CurrentPos();
        int token = decoder.ReadToken();
        if (token != previousToken)
        {
            if (previousToken != GCREFMAP_SKIP)
            {
                buf.AppendUTF8(") ");
            }
            switch (token)
            {
                case GCREFMAP_SKIP:
                    break;

                case GCREFMAP_REF:
                    buf.AppendUTF8("R(");
                    break;

                case GCREFMAP_INTERIOR:
                    buf.AppendUTF8("I(");
                    break;

                case GCREFMAP_METHOD_PARAM:
                    buf.AppendUTF8("M(");
                    break;

                case GCREFMAP_TYPE_PARAM:
                    buf.AppendUTF8("T(");
                    break;

                case GCREFMAP_VASIG_COOKIE:
                    buf.AppendUTF8("V(");
                    break;

                default:
                    // Not implemented
                    _ASSERTE(false);
            }
        }
        else if (token != GCREFMAP_SKIP)
        {
            buf.AppendUTF8(" ");
        }
        if (token != GCREFMAP_SKIP)
        {
            buf.AppendPrintf("%02x", OffsetFromGCRefMapPos(pos));
        }
        previousToken = token;
    }
    if (previousToken != GCREFMAP_SKIP)
    {
        buf.AppendUTF8(")");
    }
    buf.AppendUTF8("\n");

    minipal_log_print_info("%s", buf.GetUTF8());
}
#endif

bool CheckGCRefMapEqual(PTR_BYTE pGCRefMap, MethodDesc* pMD, bool isDispatchCell)
{
#ifdef _DEBUG
    GCRefMapBuilder gcRefMapNew;
    ComputeCallRefMap(pMD, &gcRefMapNew, isDispatchCell);

    DWORD dwFinalLength;
    PVOID pBlob = gcRefMapNew.GetBlob(&dwFinalLength);

    UINT nTokensDecoded = 0;

    GCRefMapDecoder decoderNew((BYTE *)pBlob);
    GCRefMapDecoder decoderExisting(pGCRefMap);

    bool invalidGCRefMap = false;

#ifdef TARGET_X86
    if (decoderNew.ReadStackPop() != decoderExisting.ReadStackPop())
    {
        invalidGCRefMap = true;
    }
#endif
    while (!invalidGCRefMap && !(decoderNew.AtEnd() && decoderExisting.AtEnd()))
    {
        if (decoderNew.AtEnd() != decoderExisting.AtEnd() ||
            decoderNew.CurrentPos() != decoderExisting.CurrentPos() ||
            decoderNew.ReadToken() != decoderExisting.ReadToken())
        {
            invalidGCRefMap = true;
        }
    }
    if (invalidGCRefMap)
    {
        minipal_log_print_error("GC ref map mismatch detected for method: %s::%s\n", pMD->GetMethodTable()->GetDebugClassName(), pMD->GetName());
        DumpGCRefMap("  Runtime", (BYTE *)pBlob);
        DumpGCRefMap("Crossgen2", pGCRefMap);
        _ASSERTE(false);
    }
#endif
    return true;
}

void ComputeCallRefMap(MethodDesc* pMD,
                       GCRefMapBuilder * pBuilder,
                       bool isDispatchCell)
{
#ifdef _DEBUG
    DWORD dwInitialLength = pBuilder->GetBlobLength();
    UINT nTokensWritten = 0;
#endif

    SigTypeContext typeContext(pMD);
    PCCOR_SIGNATURE pSig;
    DWORD cbSigSize;
    pMD->GetSig(&pSig, &cbSigSize);
    MetaSig msig(pSig, cbSigSize, pMD->GetModule(), &typeContext);

    bool fCtorOfVariableSizedObject = msig.HasThis() && (pMD->GetMethodTable() == g_pStringClass) && pMD->IsCtor();
    if (fCtorOfVariableSizedObject)
    {
        msig.ClearHasThis();
    }

    //
    // Shared default interface methods (i.e. virtual interface methods with an implementation) require
    // an instantiation argument. But if we're in a situation where we haven't resolved the method yet
    // we need to pretent that unresolved default interface methods are like any other interface
    // methods and don't have an instantiation argument.
    // See code:getMethodSigInternal
    //
    assert(!isDispatchCell || !pMD->RequiresInstArg() || pMD->GetMethodTable()->IsInterface());
    if (!isDispatchCell)
    {
        if (pMD->RequiresInstArg())
        {
            msig.SetHasParamTypeArg();
        }

        if (pMD->IsAsyncMethod())
        {
            msig.SetIsAsyncCall();
        }
    }

    ArgIterator argit(&msig);

    UINT nStackBytes = argit.SizeOfFrameArgumentArray();

    // Allocate a fake stack
    CQuickBytes qbFakeStack;
    qbFakeStack.AllocThrows(sizeof(TransitionBlock) + nStackBytes);
    memset(qbFakeStack.Ptr(), 0, qbFakeStack.Size());

    BYTE * pFrame = (BYTE *)qbFakeStack.Ptr();

    // Fill it in
    FakeGcScanRoots(msig, argit, pMD, pFrame);

    //
    // Encode the ref map
    //

    UINT nStackSlots;

#ifdef TARGET_X86
    UINT cbStackPop = argit.CbStackPop();
    pBuilder->WriteStackPop(cbStackPop / sizeof(TADDR));

    nStackSlots = nStackBytes / sizeof(TADDR) + NUM_ARGUMENT_REGISTERS;
#else
    nStackSlots = (sizeof(TransitionBlock) + nStackBytes - TransitionBlock::GetOffsetOfFirstGCRefMapSlot()) / TARGET_POINTER_SIZE;
#endif

    for (UINT pos = 0; pos < nStackSlots; pos++)
    {
        int ofs;

#ifdef TARGET_X86
        ofs = (pos < NUM_ARGUMENT_REGISTERS) ?
            (TransitionBlock::GetOffsetOfArgumentRegisters() + ARGUMENTREGISTERS_SIZE - (pos + 1) * sizeof(TADDR)) :
            (TransitionBlock::GetOffsetOfArgs() + (pos - NUM_ARGUMENT_REGISTERS) * sizeof(TADDR));
#else
        ofs = TransitionBlock::GetOffsetOfFirstGCRefMapSlot() + pos * TARGET_POINTER_SIZE;
#endif

        CORCOMPILE_GCREFMAP_TOKENS token = *(CORCOMPILE_GCREFMAP_TOKENS *)(pFrame + ofs);

        if (token != 0)
        {
            INDEBUG(nTokensWritten++;)
            pBuilder->WriteToken(pos, token);
        }
    }

    // We are done
    pBuilder->Flush();

#ifdef _DEBUG
    //
    // Verify that decoder produces what got encoded
    //

    DWORD dwFinalLength;
    PVOID pBlob = pBuilder->GetBlob(&dwFinalLength);

    UINT nTokensDecoded = 0;

    GCRefMapDecoder decoder((BYTE *)pBlob + dwInitialLength);

#ifdef TARGET_X86
    _ASSERTE(decoder.ReadStackPop() * sizeof(TADDR) == cbStackPop);
#endif

    while (!decoder.AtEnd())
    {
        int pos = decoder.CurrentPos();
        int token = decoder.ReadToken();

        int ofs;

#ifdef TARGET_X86
        ofs = (pos < NUM_ARGUMENT_REGISTERS) ?
            (TransitionBlock::GetOffsetOfArgumentRegisters() + ARGUMENTREGISTERS_SIZE - (pos + 1) * sizeof(TADDR)) :
            (TransitionBlock::GetOffsetOfArgs() + (pos - NUM_ARGUMENT_REGISTERS) * sizeof(TADDR));
#else
        ofs = TransitionBlock::GetOffsetOfFirstGCRefMapSlot() + pos * TARGET_POINTER_SIZE;
#endif

        if (token != 0)
        {
            _ASSERTE(*(CORCOMPILE_GCREFMAP_TOKENS *)(pFrame + ofs) == token);
            nTokensDecoded++;
        }
    }

    // Verify that all tokens got decoded.
    _ASSERTE(nTokensWritten == nTokensDecoded);
#endif // _DEBUG
}

#endif // !DACCESS_COMPILE
