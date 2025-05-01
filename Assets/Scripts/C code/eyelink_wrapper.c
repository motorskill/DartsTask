#include "eyelink.h"

#ifdef __cplusplus
extern "C" {
#endif
/* exports… */
#ifdef __cplusplus
}
#endif


__declspec(dllexport) int InitEyeLink(void)
{
    /* power‑up the DLL + clocks */
    if (!open_eyelink_system(0, ""))          /* MUST be first */
        return -1;

    /* open the TCP/IP link (‑1 == auto‑detect TSR / DLL subtype) */
    if (open_eyelink_connection(-1))          /* create buffers etc. */
        return -2;

    /* finally connect to whichever tracker is on 100.1.1.1
       (or whatever you configured with set_eyelink_address)       */
    return eyelink_open();                    /* 0 on success */
}

__declspec(dllexport) int CloseEyeLink(void)
{
    /* tell tracker to close EDF and disconnect */
    eyelink_close(1);             /* non‑zero ⇒ send CLOSE_MSG */

    close_eyelink_system();       /* stop clocks, free DLL heap */
    return 0;
}

__declspec(dllexport) int StartRecording(void)
{
    /* (1,1,1,1) = samples & events to EDF + link, check if file open */
    return start_recording(1, 1, 1, 1);
}

__declspec(dllexport) int StopRecording(void)
{
    return stop_recording();
}
