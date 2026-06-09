/*
 * Driver.c — KMDF driver entry point for A2450 HID Filter Driver.
 *
 * This is a design skeleton only.
 * Do not install.
 * Do not bind to real hardware yet.
 * Do not run on production machines.
 *
 * Driver model: HIDClass lower filter driver
 * Framework: KMDF
 * Target device: Apple Magic Keyboard A2450 (VID_05AC & PID_029C)
 *
 * The filter sits between hidusb.sys and kbdhid.sys in the driver stack:
 *
 *   kbdhid.sys (HID keyboard miniport)
 *       ↓
 *   [OpenMagicKeyboardA2450Filter.sys]  ← this driver
 *       ↓
 *   hidusb.sys (HID USB miniport)
 *       ↓
 *   USB endpoint 0x82 (Interrupt IN)
 *
 * This allows us to:
 *   1. Read the raw 10-byte HID report (including Byte 9 / Apple Fn state)
 *   2. Modify the report before kbdhid.sys processes it
 *   3. Only bind to A2450 devices (by hardware ID matching)
 */

#include <ntddk.h>
#include <wdf.h>
#include "ReportTransform.h"
#include "A2450Report.h"

/* Forward declarations */
DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD A2450FilterEvtDeviceAdd;

/*
 * DriverEntry — called when the driver is loaded.
 */
NTSTATUS
DriverEntry(
    _In_ PDRIVER_OBJECT  DriverObject,
    _In_ PUNICODE_STRING RegistryPath
)
{
    WDF_DRIVER_CONFIG config;
    NTSTATUS status;

    KdPrint(("OpenMagicKeyboardA2450Filter: DriverEntry\n"));

    WDF_DRIVER_CONFIG_INIT(&config, A2450FilterEvtDeviceAdd);

    status = WdfDriverCreate(
        DriverObject,
        RegistryPath,
        WDF_NO_OBJECT_ATTRIBUTES,
        &config,
        WDF_NO_HANDLE  /* DriverObject handle, not needed */
    );

    if (!NT_SUCCESS(status))
    {
        KdPrint(("OpenMagicKeyboardA2450Filter: WdfDriverCreate failed: 0x%08X\n", status));
    }

    return status;
}

/*
 * A2450FilterEvtDeviceAdd — called when PnP manager discovers a matching device.
 *
 * The INF file specifies which hardware IDs this driver binds to.
 * For A2450: HID\VID_05AC&PID_029C
 *
 * TODO: Create device object, set up filter I/O callbacks.
 */
NTSTATUS
A2450FilterEvtDeviceAdd(
    _In_ WDFDRIVER   Driver,
    _Inout_ PWDFDEVICE_INIT DeviceInit
)
{
    UNREFERENCED_PARAMETER(Driver);

    KdPrint(("OpenMagicKeyboardA2450Filter: DeviceAdd\n"));

    /*
     * As a HID lower filter, we need to:
     *   1. Set ourselves as a filter (not a function driver)
     *   2. Forward IRPs we don't care about to the next driver
     *   3. Intercept IOCTL_HID_READ_REPORT to transform incoming reports
     *
     * TODO: Implement device creation and I/O queue setup.
     * See Device.c for the planned implementation.
     */

    return STATUS_SUCCESS;
}
