/*
 * ReportTransform.h — A2450 HID Report transformation interface.
 *
 * This is a design skeleton only.
 * Do not install.
 * Do not bind to real hardware yet.
 * Do not run on production machines.
 */

#pragma once

#include <ntddk.h>
#include "A2450Report.h"

/*
 * Driver runtime state that persists across report transformations.
 * In a real driver, this would be stored in the device context.
 */
typedef struct _A2450_TRANSFORM_STATE
{
    BOOLEAN SwapEnabled;        /* Master switch for Fn/Ctrl swap */
    BOOLEAN ClearAppleFnByte;   /* Whether to zero out Byte 9 */
    BOOLEAN EnableFnLayer;      /* Whether FnLayer remapping is active */
    BOOLEAN FnLayerActive;      /* Current FnLayer state (derived from physical Ctrl) */
} A2450_TRANSFORM_STATE, *PA2450_TRANSFORM_STATE;

/*
 * Initialize transform state with default values.
 */
VOID
A2450TransformStateInit(_Out_ PA2450_TRANSFORM_STATE State);

/*
 * Transform a 10-byte A2450 keyboard HID report in-place.
 *
 * Transformation rules:
 *   1. Physical Fn (Byte 9 bit 1) → set Left Ctrl in Byte 1
 *   2. Physical Left Ctrl (Byte 1 bit 0) → remove from Byte 1, activate FnLayer
 *   3. FnLayer active → remap Backspace/arrows to Delete/Home/End/PageUp/PageDown
 *
 * Returns TRUE if the report was modified, FALSE if passed through unchanged.
 */
BOOLEAN
A2450TransformKeyboardReport(
    _Inout_updates_bytes_(ReportLength) UCHAR* Report,
    _In_ size_t ReportLength,
    _In_ PA2450_TRANSFORM_STATE State
);

/*
 * Remap a single key usage code according to FnLayer table.
 * Returns the remapped usage, or the original if no mapping exists.
 *
 * FnLayer mappings:
 *   Backspace (0x2A) → Delete (0x4C)
 *   Up        (0x52) → PageUp (0x4B)
 *   Down      (0x51) → PageDown (0x4E)
 *   Left      (0x50) → Home    (0x4A)
 *   Right     (0x4F) → End     (0x4D)
 *
 * F7-F12 media keys need Consumer Control Usage Page 0x0C
 * and should be handled in a future version.
 */
UCHAR
A2450RemapFnLayerKey(_In_ UCHAR Usage);
