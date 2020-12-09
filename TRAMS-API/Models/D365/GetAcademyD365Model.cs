﻿using Newtonsoft.Json;
using System;

namespace API.Models.D365
{
    public class GetAcademyD365Model
    {
        [JsonProperty("accountid")]
        public Guid Id { get; set; }

        [JsonProperty("_parentaccountid_value")]
        public Guid ParentTrustId { get; set; }

        [JsonProperty("name")]
        public string AcademyName { get; set; }

        [JsonProperty("sip_urn")]
        public string Urn { get; set; }

        [JsonProperty("_sip_establishmenttypeid_value@OData.Community.Display.V1.FormattedValue")]
        public string EstablishmentType { get; set; }

        [JsonProperty("sip_localauthoritynumber")]
        public string LocalAuthorityNumber { get; set; }

        [JsonProperty("_sip_localauthorityareaid_value@OData.Community.Display.V1.FormattedValue")]
        public string LocalAuthorityName { get; set; }

        [JsonProperty("_sip_religiouscharacterid_value@OData.Community.Display.V1.FormattedValue")]
        public string ReligiousCharacter { get; set; }

        [JsonProperty("_sip_dioceseid_value@OData.Community.Display.V1.FormattedValue")]
        public string DioceseName { get; set; }

        [JsonProperty("_sip_religiousethosid_value@OData.Community.Display.V1.FormattedValue")]
        public string ReligiousEthos { get; set; }

        [JsonProperty("sip_ofstedrating@OData.Community.Display.V1.FormattedValue")]
        public string OftstedRating { get; set; }

        [JsonProperty("sip_ofstedinspectiondate")]
        public DateTime? OfstedInspectionDate { get; set; }

        //        @odata.etag:"W/"286817758"",
        //_sip_dioceseid_value @OData.Community.Display.V1.FormattedValue:"Not applicable",
        //_sip_dioceseid_value @Microsoft.Dynamics.CRM.associatednavigationproperty:"sip_Dioceseid",
        //_sip_dioceseid_value @Microsoft.Dynamics.CRM.lookuplogicalname:"sip_diocese",
        //_sip_dioceseid_value:"b4583fc5-ddff-e911-a811-000d3a4aaea6",
        //_sip_religiouscharacterid_value @OData.Community.Display.V1.FormattedValue:"None",
        //_sip_religiouscharacterid_value @Microsoft.Dynamics.CRM.associatednavigationproperty:"sip_ReligiousCharacterID",
        //_sip_religiouscharacterid_value @Microsoft.Dynamics.CRM.lookuplogicalname:"sip_religiouscharacter",
        //_sip_religiouscharacterid_value:"9a825db8-ddff-e911-a811-000d3a4aa344",
        //_sip_religiousethosid_value @OData.Community.Display.V1.FormattedValue:"Does not apply",
        //_sip_religiousethosid_value @Microsoft.Dynamics.CRM.associatednavigationproperty:"sip_ReligiousEthosID",
        //_sip_religiousethosid_value @Microsoft.Dynamics.CRM.lookuplogicalname:"sip_religiousethos",
        //_sip_religiousethosid_value:"f9ea39dc-ddff-e911-a811-000d3a4aa344",
        //accountid:"de43b9c4-eaa0-e911-a83f-000d3a3852a3"
        //_sip_localauthorityareaid_value @OData.Community.Display.V1.FormattedValue:"Hertfordshire",
    }
}