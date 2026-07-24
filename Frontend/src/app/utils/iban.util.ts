export class IbanUtil {
  public static BANK_CODE = '00062';

  public static generateAccountNo(): string {
    let randomDigits = '';
    for (let i = 0; i < 12; i++) {
      randomDigits += Math.floor(Math.random() * 10).toString();
    }
    return '1000' + randomDigits;
  }

  public static generateTrIban(accountNo: string): string {
    if (!accountNo) {
      accountNo = this.generateAccountNo();
    }

    let digitsOnly = accountNo.replace(/[^\d]/g, '');
    if (!digitsOnly) {
      digitsOnly = '1000000000000001';
    }

    if (digitsOnly.length > 16) {
      digitsOnly = digitsOnly.slice(-16);
    } else {
      digitsOnly = digitsOnly.padStart(16, '0');
    }

    const bban = this.BANK_CODE + '0' + digitsOnly;
    const modString = bban + '292700';

    let remainder = 0;
    for (let i = 0; i < modString.length; i++) {
      remainder = (remainder * 10 + parseInt(modString.charAt(i), 10)) % 97;
    }

    const checkDigits = 98 - remainder;
    const kk = checkDigits < 10 ? '0' + checkDigits : checkDigits.toString();

    return 'TR' + kk + bban;
  }

  public static validateTrIban(iban: string): boolean {
    if (!iban) return false;
    const cleanIban = iban.trim().toUpperCase().replace(/\s+/g, '');
    if (cleanIban.length !== 26 || !cleanIban.startsWith('TR')) {
      return false;
    }

    const numericPart = cleanIban.substring(2);
    if (!/^\d{24}$/.test(numericPart)) {
      return false;
    }

    const bban = cleanIban.substring(4, 26);
    const kk = cleanIban.substring(2, 4);
    const modString = bban + '2927' + kk;

    let remainder = 0;
    for (let i = 0; i < modString.length; i++) {
      remainder = (remainder * 10 + parseInt(modString.charAt(i), 10)) % 97;
    }

    return remainder === 1;
  }
}
