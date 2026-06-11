/*
 * Device.h — A2450 device context and accessor declarations.
 *
 * Design / build skeleton only.
 * Do not install.  Do not bind to real hardware.
 */

#pragma once

#include <ntddk.h>
#include <wdf.h>
#include "ReportTransform.h"

/*
 * Device context — per-device state.
 * Each A2450 keyboard instance gets its own context.
 */
typedef struct _A2450_DEVICE_CONTEXT
{
    WDFDEVICE               Device;
    WDFIOTARGET             IoTarget;
    A2450_TRANSFORM_STATE   TransformState;

    /* Statistics for debugging. */
    ULONG64                 ReportsTransformed;
    ULONG64                 ReportsPassedThrough;
} A2450_DEVICE_CONTEXT, *PA2450_DEVICE_CONTEXT;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(A2450_DEVICE_CONTEXT, A2450GetDeviceContext);
