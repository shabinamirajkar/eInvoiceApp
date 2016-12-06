    function SaveApproversGrid() {
        var grid = $("#EditableApproversGrid").data("kendoGrid"),
         parameterMap = grid.dataSource.transport.parameterMap;

         //get the new and the updated records
         var currentData = grid.dataSource.data();
         var updatedRecords = [];
         var newRecords = [];

         for (var i = 0; i < currentData.length; i++) {
             if (currentData[i].isNew()) {
                 //this record is new
                 newRecords.push(currentData[i].toJSON());
             } else if (currentData[i].dirty) {
                 updatedRecords.push(currentData[i].toJSON());
             }
         }
         //this records are deleted
         var deletedRecords = [];
         for (var i = 0; i < grid.dataSource._destroyed.length; i++) {
             deletedRecords.push(grid.dataSource._destroyed[i].toJSON());
         }
         var data = {};
         $.extend(data, parameterMap({ updated: updatedRecords }), parameterMap({ deleted: deletedRecords }), parameterMap({ new: newRecords }));
         $.ajax({
             url: '@Url.Action("AP_Save", "RoutingDetails", Request.RequestContext.RouteData.Values)',
             data: data,
             type: "POST",
             error: function () {
             },
             success: function () {
               
                 grid.dataSource._destroyed = [];
                 //refresh the grid - optional
                 grid.dataSource.read();
    
              }
         });
     }


   // Saving Approvers Grid on click of Submit
    function SaveApproversGridCallBack(callback)
    {
        var grid = $("#EditableApproversGrid").data("kendoGrid"),
        parameterMap = grid.dataSource.transport.parameterMap;

        //get the new and the updated records
        var currentData = grid.dataSource.data();
        var updatedRecords = [];
        var newRecords = [];

        for (var i = 0; i < currentData.length; i++) {
           if (currentData[i].isNew()) {
              //this record is new
             newRecords.push(currentData[i].toJSON());
           } else if (currentData[i].dirty) {
            updatedRecords.push(currentData[i].toJSON());
          }
        }
           //this records are deleted
           var deletedRecords = [];
           for (var i = 0; i < grid.dataSource._destroyed.length; i++) {
               deletedRecords.push(grid.dataSource._destroyed[i].toJSON());
           }
           var data = {};
           $.extend(data, parameterMap({ updated: updatedRecords }), parameterMap({ deleted: deletedRecords }), parameterMap({ new: newRecords }));
           $.ajax({
               url: 'http://localhost/eInvoiceAutomationWeb/RoutingDetails/Approvers_CreateUpdateDelete',
               data: data,
               type: "POST"
           })
           .done(callback)
           .fail(function () {

           });
    }

   
    // Saving Accounting CostCodes Grid on click of Submit
    function SaveAccountingCostCodesGridCallBack(callback) {
         var grid = $("#ModifyAccountingGrid").data("kendoGrid"),
        parameterMap = grid.dataSource.transport.parameterMap;

        //get the new and the updated records
        var currentData = grid.dataSource.data();
        var updatedRecords = [];
        var newRecords = [];

        for (var i = 0; i < currentData.length; i++) {
            if (currentData[i].isNew()) {
                //this record is new
                newRecords.push(currentData[i].toJSON());
            } else if (currentData[i].dirty) {
                updatedRecords.push(currentData[i].toJSON());
            }
        }
        //this records are deleted
        var deletedRecords = [];
        for (var i = 0; i < grid.dataSource._destroyed.length; i++) {
            deletedRecords.push(grid.dataSource._destroyed[i].toJSON());
        }
        var data = {};
        $.extend(data, parameterMap({ updated: updatedRecords }), parameterMap({ deleted: deletedRecords }), parameterMap({ new: newRecords }));
        $.ajax({
            url: 'http://localhost/eInvoiceAutomationWeb/PurchaseOrderDetails/ModifyAccountingCostCodes_CreateUpdateDelete',
            data: data,
            type: "POST"
        })
        .done(callback)
        .fail(function () {
        });
    }

   