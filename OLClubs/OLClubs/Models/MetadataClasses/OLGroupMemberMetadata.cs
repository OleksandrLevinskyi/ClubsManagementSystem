/*
 * OLGroupMemberMetadata.cs
 * Description: GroupMember partial class and metadata
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/03/2020
 */

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OLClubs.Models
{
    /// <summary>
    /// partial class of GroupMember model with Metadata annotation added for data formatting
    /// </summary>
    [ModelMetadataType(typeof(OLGroupMemberMetadata))]
    public partial class GroupMember { }

    /// <summary>
    /// metadata class for the GroupMember model to format the dates
    /// </summary>
    public class OLGroupMemberMetadata
    {
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateJoined { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateLeft { get; set; }
    }
}
