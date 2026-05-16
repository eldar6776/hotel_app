export interface GuestDto {
  id: string
  firstName: string
  lastName: string
  dateOfBirth: string | null
  gender: string | null
  email: string | null
  phone: string | null
  address: string | null
  city: string | null
  postalCode: string | null
  countryId: string | null
  countryName: string | null
  isCompany: boolean
  companyName: string | null
  vatNumber: string | null
  gdprConsentGiven: boolean
  notes: string | null
  isActive: boolean
  createdAt: string
  documents: GuestDocumentDto[]
}

export interface GuestDocumentDto {
  id: string
  guestId: string
  documentType: string
  documentNumber: string
  issuingCountry: string
  expiryDate: string | null
  isVerified: boolean
}
