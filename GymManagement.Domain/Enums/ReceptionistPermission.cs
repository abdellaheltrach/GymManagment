namespace GymManagement.Domain.Enums
{
    // This attribute indicates that each value represents a bit field, allowing for bitwise operations and multiple values can be combined like StandardDesk.
    [Flags]
    public enum ReceptionistPermission
    {
        // << N is Bitwise Left Shift 
        //1 << 8 means: take 1 in binary, shift it left 8 positions
        None = 0,

        RegisterTrainees = 1 << 0,      //   1  000000001 
        AssignMemberships = 1 << 1,     //   2  000000010
        RecordPayments = 1 << 2,        //   4  000000100
        FreezeMemberships = 1 << 3,     //   8  000001000
        CancelMemberships = 1 << 4,     //  16  000010000
        SuspendMemberships = 1 << 5,    //  32  000100000
        CheckInTrainees = 1 << 6,       //  64  001000000
        ViewFinancialReports = 1 << 7,  // 128  010000000
        EditTraineeProfile = 1 << 8,    // 256  100000000

        // ── Preset bundles (convenience — never stored directly) ──────────
        StandardDesk = RegisterTrainees | AssignMemberships |
                       RecordPayments | FreezeMemberships | CheckInTrainees,  //equal to 109 (bits 0,1,2,3,6 set)  001101101

        FullAccess = RegisterTrainees | AssignMemberships | RecordPayments |
                     FreezeMemberships | CancelMemberships | SuspendMemberships |
                     CheckInTrainees | ViewFinancialReports | EditTraineeProfile // equal to 511 (all bits set)  111111111
    }
}
