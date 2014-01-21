using Manager.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ManagerConsole.ViewModel
{
    public class UserModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }
    }
    //public class AccessPermissionTree
    //{
    //    public List<CategoryNode> CategoryNodes { get; set; }

    //    public AccessPermissionTree()
    //    {
    //        CategoryNodes = new List<CategoryNode>();
    //    }
    //}

    //public class CategoryNode
    //{
    //    public ModuleCategoryType CategoryType { get; set; }
    //    public bool CategoryAllow { get; private set; }
    //    public bool CategoryDeny { get; private set; }
    //    public List<ModuleNode> ModuleNodes { get; set; }

    //    public CategoryNode()
    //    {
    //        ModuleNodes = new List<ModuleNode>();
    //    }

    //    public void Set(bool isAllow)
    //    {
    //        if (isAllow)
    //        {
    //            CategoryAllow = true;
    //            CategoryDeny = false;
    //        }
    //        else
    //        {
    //            CategoryDeny = true;
    //            CategoryAllow = false;
    //        }
    //    }
    //}

    //public class ModuleNode
    //{
    //    public ModuleType Type { get; set; }
    //    public bool ModuleAllow { get; private set; }
    //    public bool ModuleDeny { get; private set; }
    //    public List<OperationNode> OperationNodes { get; set; }

    //    public ModuleNode()
    //    {
    //        OperationNodes = new List<OperationNode>();
    //    }

    //    public void Set(bool isAllow)
    //    {
    //        if (isAllow)
    //        {
    //            ModuleAllow = true;
    //            ModuleDeny = false;
    //        }
    //        else
    //        {
    //            ModuleAllow = false;
    //            ModuleDeny = true;
    //        }
    //    }
    //}

    //public class OperationNode
    //{
    //    public int Id { get; set; }
    //    public bool OperationAllow { get; private set; }
    //    public bool OperationDeny { get; private set; }
    //    public string OperationDescription { get; set; }

    //    public void Set(bool isAllow)
    //    {
    //        if (isAllow)
    //        {
    //            OperationAllow = true;
    //            OperationDeny = false;
    //        }
    //        else
    //        {
    //            OperationAllow = false;
    //            OperationDeny = true;
    //        }
    //    }
    //}

    //public class DataPermissionTree
    //{
    //    public List<ExchangeSystemNode> ExChangeSystemNodes { get; set; }

    //    public DataPermissionTree()
    //    {
    //        ExChangeSystemNodes = new List<ExchangeSystemNode>();
    //    }
    //}

    //public class ExchangeSystemNode
    //{
    //    public string ExChangeCode { get; set; }
    //    public List<DataObjectTypeNode> DataObjectTypeNodes { get; set; }

    //    public ExchangeSystemNode()
    //    {
    //        DataObjectTypeNodes = new List<DataObjectTypeNode>();
    //    }
    //}

    //public class DataObjectTypeNode
    //{
    //    public DataObjectType Type { get; set; }
    //    public List<DataObjectNode> DataObjectNodes { get; set; }

    //    public DataObjectTypeNode()
    //    {
    //        DataObjectNodes = new List<DataObjectNode>();
    //    }
    //}

    //public class DataObjectNode
    //{
    //    public Guid DataObjectId { get; set; }
    //    public string Decription { get; set; }
    //}

    public class RoleGridData
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsAllowEdit { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowAdd { get; set; }

        public RoleGridData(RoleData role, bool isAllowAdd, bool isAllowDelete, bool isAllowEdit)
        {
            RoleId = role.RoleId;
            RoleName = role.RoleName;
            IsAllowAdd = isAllowAdd;
            IsAllowDelete = isAllowDelete;
            IsAllowEdit = isAllowEdit;
        }
    }

    #region FunctionPermission
    public class FunctionGridData
    {
        public ObservableCollection<CategoryGridData> CategoryDatas { get; set; }
        public FunctionGridData()
        {
            CategoryDatas = new ObservableCollection<CategoryGridData>();
        }

        public bool CheckData()
        {
            bool isValid = true;
            foreach (CategoryGridData item in CategoryDatas)
            {
                if (!item.CheckDataIsValid())
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        public void CastFunctionToGridData(List<RoleFunctonPermission> roleData, List<RoleFunctonPermission> allFunction)
        {
            ObservableCollection<CategoryGridData> data = new ObservableCollection<CategoryGridData>();
            foreach (RoleFunctonPermission item in allFunction)
            {
                if (item.Level == 1)
                {
                    CategoryGridData row = new CategoryGridData();
                    row.CategoryId = item.FunctionId;
                    row.CategoryName = item.Description;
                    RoleFunctonPermission category = roleData.SingleOrDefault(f => f.FunctionId == item.FunctionId);
                    if (category != null)
                    {
                        row.IsCategoryAllow = category.IsAllow;
                        row.IsCategoryDeny = !category.IsAllow;
                    }
                    else
                    {
                        row.IsCategoryAllow = false;
                        row.IsCategoryDeny = false;
                    }
                    List<RoleFunctonPermission> modules = allFunction.FindAll(delegate(RoleFunctonPermission function)
                    {
                        return function.ParentId == item.FunctionId;
                    });
                    foreach (RoleFunctonPermission module in modules)
                    {
                        ModuleData moduleRow = new ModuleData();
                        moduleRow.ModuleId = module.FunctionId;
                        moduleRow.ModuleName = module.Description;
                        RoleFunctonPermission modulePermission = roleData.SingleOrDefault(f => f.FunctionId == module.FunctionId);
                        if (modulePermission != null)
                        {
                            moduleRow.IsModuleAllow = modulePermission.IsAllow;
                            moduleRow.IsModuleDeny = !modulePermission.IsAllow;
                        }
                        else
                        {
                            if (category != null)
                            {
                                if (category.IsAllow)
                                {
                                    moduleRow.IsModuleAllow = null;
                                    moduleRow.IsModuleDeny = false;
                                }
                                else
                                {
                                    moduleRow.IsModuleAllow = false;
                                    moduleRow.IsModuleDeny = null;
                                }
                            }
                            else
                            {
                                moduleRow.IsModuleAllow = false;
                                moduleRow.IsModuleDeny = false;
                            }
                        }
                        List<RoleFunctonPermission> operations = allFunction.FindAll(delegate(RoleFunctonPermission access)
                        {
                            return access.ParentId == module.FunctionId;
                        });
                        foreach (RoleFunctonPermission operation in operations)
                        {
                            OperationData operationRow = new OperationData();
                            operationRow.FunctionId = operation.FunctionId;
                            operationRow.Description = operation.Description;
                            operationRow.IsAllow = operation.IsAllow;
                            operationRow.IsDeny = !operation.IsAllow;
                            RoleFunctonPermission operationPermission = roleData.SingleOrDefault(f => f.FunctionId == operation.FunctionId);
                            if (operationPermission != null)
                            {
                                operationRow.IsAllow = operationPermission.IsAllow;
                                operationRow.IsDeny = !operationPermission.IsAllow;
                            }
                            else
                            {
                                if (modulePermission != null)
                                {
                                    if (modulePermission.IsAllow)
                                    {
                                        operationRow.IsAllow = null;
                                        operationRow.IsDeny = false;
                                    }
                                    else
                                    {
                                        operationRow.IsAllow = false;
                                        operationRow.IsDeny = null;
                                    }
                                }
                                else if (category != null)
                                {
                                    if (category.IsAllow)
                                    {
                                        operationRow.IsAllow = null;
                                        operationRow.IsDeny = false;
                                    }
                                    else
                                    {
                                        operationRow.IsAllow = false;
                                        operationRow.IsDeny = null;
                                    }
                                }
                                else
                                {
                                    operationRow.IsAllow = false;
                                    operationRow.IsDeny = false;
                                }
                            }
                            moduleRow.OperationDatas.Add(operationRow);
                        }
                        row.ModuleDatas.Add(moduleRow);
                    }
                    data.Add(row);
                }
            }
            CategoryDatas = data;
        }

        public List<RoleFunctonPermission> CastGridDataToFunction()
        {
            List<RoleFunctonPermission> roleFunctionPermissions = new List<RoleFunctonPermission>();
            foreach (CategoryGridData category in CategoryDatas)
            {
                if (category.IsCategoryAllow == true || category.IsCategoryDeny == true)
                {
                    RoleFunctonPermission function = new RoleFunctonPermission();
                    function.FunctionId = category.CategoryId;
                    function.ParentId = 1;
                    function.Description = category.CategoryName;
                    function.Level = 1;
                    if (category.IsCategoryAllow == true)
                    {
                        function.IsAllow = true;
                    }
                    if (category.IsCategoryDeny == true)
                    {
                        function.IsAllow = false;
                    }
                    roleFunctionPermissions.Add(function);
                }
                foreach (ModuleData module in category.ModuleDatas)
                {
                    if (module.IsModuleAllow == true || module.IsModuleDeny == true)
                    {
                        RoleFunctonPermission function = new RoleFunctonPermission();
                        function.FunctionId = module.ModuleId;
                        function.Description = module.ModuleName;
                        function.ParentId = category.CategoryId;
                        function.Level = 2;
                        if (module.IsModuleAllow == true)
                        {
                            function.IsAllow = true;
                        }
                        if (module.IsModuleDeny == true)
                        {
                            function.IsAllow = false;
                        }
                        roleFunctionPermissions.Add(function);
                    }
                    foreach (OperationData operation in module.OperationDatas)
                    {
                        if (operation.IsAllow == true || operation.IsDeny == true)
                        {
                            RoleFunctonPermission function = new RoleFunctonPermission();
                            function.FunctionId = operation.FunctionId;
                            function.Description = operation.Description;
                            function.ParentId = module.ModuleId;
                            function.Level = 3;
                            if (operation.IsAllow == true)
                            {
                                function.IsAllow = true;
                            }
                            if (operation.IsDeny == true)
                            {
                                function.IsAllow = false;
                            }
                            roleFunctionPermissions.Add(function);
                        }
                    }
                }
            }
            return roleFunctionPermissions;
        }
    }

    public class CategoryGridData : INotifyPropertyChanged
    {
        #region private
        private bool _isCategoryAllow;
        private bool _isCategoryDeny;
        #endregion

        #region public
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsCategoryAllow
        {
            get
            {
                return _isCategoryAllow;
            }
            set
            {
                if (value == true)
                {
                    _isCategoryAllow = value;
                    _isCategoryDeny = !value;
                    this.SetChildDeafuolt(null, false);
                }
                else
                {
                    _isCategoryAllow = value;
                }
                if (_isCategoryDeny != true)
                {
                    this.SetChildIsAllow(value);
                }
                this.NotifyPropertyChanged("IsCategoryAllow");
                this.NotifyPropertyChanged("IsCategoryDeny");
            }
        }
        public bool IsCategoryDeny
        {
            get
            {
                return _isCategoryDeny;
            }
            set
            {
                if (value == true)
                {
                    _isCategoryDeny = value;
                    _isCategoryAllow = !value;
                    this.SetChildDeafuolt(false, null);
                }
                else
                {
                    _isCategoryDeny = value;
                }
                if (_isCategoryAllow != true)
                {
                    this.SetChildIsDeny(value);
                }
                this.NotifyPropertyChanged("IsCategoryAllow");
                this.NotifyPropertyChanged("IsCategoryDeny");
            }
        }
        public ObservableCollection<ModuleData> ModuleDatas { get; set; }
        #endregion

        #region Constroctor
        public CategoryGridData()
        {
            ModuleDatas = new ObservableCollection<ModuleData>();
        }
        #endregion

        #region event
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region method
        private void SetChildIsAllow(bool? isAllow)
        {
            if (isAllow == false)
            {
                foreach (ModuleData module in ModuleDatas)
                {
                    if (module.IsModuleAllow == null || module.IsModuleDeny == null || module.IsModuleDeny == module.IsModuleAllow)
                    {
                        module.SetDeafult(false, false);
                        module.SetPermission(false, false);
                    }
                }
            }
            else
            {
                foreach (ModuleData module in ModuleDatas)
                {
                    if (module.IsModuleAllow == null || module.IsModuleDeny == null || module.IsModuleDeny == module.IsModuleAllow)
                    {
                        module.SetDeafult(null, false);
                        module.SetPermission(null, false);
                    }
                }
            }
        }

        private void SetChildIsDeny(bool? isDeny)
        {
            if (isDeny == false)
            {
                foreach (ModuleData module in ModuleDatas)
                {
                    if (module.IsModuleAllow == null || module.IsModuleDeny == null || module.IsModuleDeny == module.IsModuleAllow)
                    {
                        module.SetDeafult(false, false);
                        module.SetPermission(_isCategoryAllow, false);
                    }
                }
            }
            else
            {
                foreach (ModuleData module in ModuleDatas)
                {
                    if (module.IsModuleAllow == null || module.IsModuleDeny == null || module.IsModuleDeny == module.IsModuleAllow)
                    {
                        module.SetDeafult(false, null);
                        module.SetPermission(false, null);
                    }
                }
            }
        }

        private void SetChildDeafuolt(bool? isAllow, bool? isDeny)
        {
            foreach (ModuleData module in ModuleDatas)
            {
                module.SetDeafult(isAllow, isDeny);
            }
        }

        public bool CheckDataIsValid()
        {
            bool isValid = true;
            if (IsCategoryAllow == false && IsCategoryDeny == false)
            {
                foreach (ModuleData module in ModuleDatas)
                {
                    if (module.IsModuleAllow == false && module.IsModuleDeny == false)
                    {
                        foreach (OperationData operation in module.OperationDatas)
                        {
                            if (operation.IsAllow == false && operation.IsDeny == false)
                            {
                                isValid = false;
                            }
                        }
                    }
                }
            }
            return isValid;
        }
        #endregion
    }

    public class ModuleData : INotifyPropertyChanged
    {
        #region private
        private bool? _isModuleAllow = false;
        private bool? _isModuleDeny = false;
        private bool? _deafultAllow = false;
        private bool? _deafultDeny = false;
        #endregion

        #region public
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool? IsModuleAllow
        {
            get
            {
                return _isModuleAllow;
            }
            set
            {
                if (_isModuleAllow != null)
                {
                    if (value == true)
                    {
                        _isModuleAllow = value;
                        _isModuleDeny = !value;
                        this.SetChildDeafult(null, false);
                    }
                    else
                    {
                        this.SetChildDeafult(_deafultAllow, _deafultDeny);
                        _isModuleAllow = value;
                    }
                    if (_isModuleDeny == false)
                    {
                        this.SetChildIsAllow(value);
                        if (_isModuleAllow == false)
                        {
                            _isModuleDeny = _deafultDeny;
                        }
                    }
                    this.NotifyPropertyChanged("IsModuleAllow");
                    this.NotifyPropertyChanged("IsModuleDeny");
                }
            }
        }
        public bool? IsModuleDeny
        {
            get
            {
                return _isModuleDeny;
            }
            set
            {
                if (_isModuleDeny != null)
                {
                    if (value == true)
                    {
                        _isModuleDeny = value;
                        _isModuleAllow = !value;
                        this.SetChildDeafult(false, null);
                    }
                    else
                    {
                        this.SetChildDeafult(_deafultAllow, _deafultDeny);
                        _isModuleDeny = value;
                    }
                    if (_isModuleAllow == false)
                    {
                        this.SetChildIsDeny(value);
                        if (_isModuleDeny == false)
                        {
                            _isModuleAllow = _deafultAllow;
                        }
                    }
                    this.NotifyPropertyChanged("IsModuleAllow");
                    this.NotifyPropertyChanged("IsModuleDeny");
                }
            }
        }

        public ObservableCollection<OperationData> OperationDatas { get; set; }

        #endregion

        #region event
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region Constructor
        public ModuleData()
        {
            OperationDatas = new ObservableCollection<OperationData>();
        }
        #endregion

        #region method
        private void SetChildIsAllow(bool? isAllow)
        {
            if (isAllow == false)
            {
                foreach (OperationData operation in OperationDatas)
                {
                    if (operation.IsAllow == null || operation.IsDeny == null || operation.IsAllow == operation.IsDeny)
                    {
                        operation.SetPermission(false, _deafultDeny);
                        operation.ResetDeafult(false, _deafultDeny);
                    }
                }
            }
            else
            {
                foreach (OperationData operation in OperationDatas)
                {
                    if (operation.IsAllow == null || operation.IsDeny == null || operation.IsAllow == operation.IsDeny)
                    {
                        operation.SetPermission(null, false);
                        operation.ResetDeafult(null, false);
                    }
                }
            }
        }

        private void SetChildIsDeny(bool? isDeny)
        {
            if (isDeny == false)
            {
                foreach (OperationData operation in OperationDatas)
                {
                    if (operation.IsAllow == null || operation.IsDeny == null || operation.IsAllow == operation.IsDeny)
                    {
                        operation.SetPermission(_deafultAllow, false);
                        operation.ResetDeafult(_deafultAllow, false);
                    }
                }
            }
            else
            {
                foreach (OperationData operation in OperationDatas)
                {
                    if (operation.IsAllow == null || operation.IsDeny == null || operation.IsAllow == operation.IsDeny)
                    {
                        operation.SetPermission(false, null);
                        operation.ResetDeafult(false, null);
                    }
                }
            }
        }

        public void SetPermission(bool? isModuleAllow, bool? isModuleDeny)
        {
            this._isModuleAllow = isModuleAllow;
            this._isModuleDeny = isModuleDeny;
            if (_isModuleAllow !=isModuleAllow)
            {
                this.SetChildIsAllow(isModuleAllow);
            }
            if (_isModuleDeny == isModuleDeny)
            {
                this.SetChildIsDeny(isModuleDeny);
            }
            this.NotifyPropertyChanged("IsModuleAllow");
            this.NotifyPropertyChanged("IsModuleDeny");
        }

        public void SetDeafult(bool? isAllow, bool? isDeny)
        {
            this._deafultAllow = isAllow;
            this._deafultDeny = isDeny;
        }

        public void SetChildDeafult(bool? isallow, bool? isDeny)
        {
            foreach (OperationData operation in OperationDatas)
            {
                operation.ResetDeafult(isallow, isDeny);
            }
        }
        #endregion
    }

    public class OperationData : INotifyPropertyChanged
    {
        #region private
        private bool? _isAllow = false;
        private bool? _isDeny = false;
        private bool? _deafultAllow = false;
        private bool? _deafultDeny = false;
        #endregion

        #region public
        public int FunctionId { get; set; }
        public string Description { get; set; }
        public bool? IsAllow
        {
            get
            {
                return _isAllow;
            }
            set
            {
                if (_isAllow != null)
                {
                    if (value == true)
                    {
                        if (_deafultAllow == null)
                        {
                            _isAllow = null;
                        }
                        else
                        {
                            _isAllow = value;
                        }
                        if (_isDeny == null)
                        {
                            _deafultDeny = null;
                        }
                        _isDeny = !value;

                    }
                    else
                    {
                        if (value == false && _deafultDeny == null)
                        {
                            _isDeny = _deafultDeny;
                        }
                        _isAllow = value;
                    }
                    this.NotifyPropertyChanged("IsAllow");
                    this.NotifyPropertyChanged("IsDeny");
                }
            }
        }
        public bool? IsDeny
        {
            get
            {
                return _isDeny;
            }
            set
            {
                if (_isDeny != null)
                {
                    if (value == true)
                    {
                        if (_deafultDeny == null)
                        {
                            _isDeny = null;
                        }
                        else
                        {
                            _isDeny = value;
                        }
                        if (_isAllow == null)
                        {
                            _deafultAllow = null;
                        }
                        _isAllow = !value;
                    }
                    else
                    {
                        if (value == false && _deafultAllow == null)
                        {
                            _isAllow = _deafultAllow;
                        }
                        _isDeny = value;
                    }
                    this.NotifyPropertyChanged("IsAllow");
                    this.NotifyPropertyChanged("IsDeny");
                }
            }
        }
        #endregion

        #region event
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region method
        public void SetPermission(bool? isAllow, bool? isDeny)
        {
            this._isAllow = isAllow;
            this._isDeny = isDeny;
            this.NotifyPropertyChanged("IsAllow");
            this.NotifyPropertyChanged("IsDeny");
        }

        public void ResetDeafult(bool? isAllow, bool? isDeny)
        {
            this._deafultAllow = isAllow;
            this._deafultDeny = isDeny;
        }
        #endregion
    } 
    #endregion

    #region DataPermission
    public class DataPermissionGridData
    {
        public ObservableCollection<ExchangeGridData> IExchangeCodes { get; set; }

        public DataPermissionGridData()
        {
            IExchangeCodes = new ObservableCollection<ExchangeGridData>();
        }

        public bool CheckData()
        {
            bool isValid = true;
            foreach (ExchangeGridData exchange in IExchangeCodes)
            {
                isValid = exchange.CheckDataIsValid();
            }
            return isValid;
        }

        public void CastDataPermissionToGridData(List<RoleDataPermission> dataPermissions, List<RoleDataPermission> allDatas)
        {
            ObservableCollection<ExchangeGridData> data = new ObservableCollection<ExchangeGridData>();
            List<string> ExchangeCodes = new List<string>();
            foreach (RoleDataPermission item in allDatas)
            {
                if (!ExchangeCodes.Contains(item.ExchangeCode))
                {
                    ExchangeCodes.Add(item.ExchangeCode);
                }
            }
            foreach (string item in ExchangeCodes)
            {
                ExchangeGridData exchange = new ExchangeGridData();
                exchange.ExchangeCode = item;
                RoleDataPermission exchangeNode = dataPermissions.SingleOrDefault(d => d.Code == item && d.ParentId == 2);
                DataObjectTypeGridData account = new DataObjectTypeGridData();
                account.DataObjectType = "Account";
                DataObjectTypeGridData instrument = new DataObjectTypeGridData();
                instrument.DataObjectType = "Instrument";
                if (exchangeNode != null)
                {
                    exchange.ExchangeId = exchangeNode.PermissionId;
                    exchange.IsExchangeAllow = exchangeNode.IsAllow;
                    exchange.IsExchangeDeny = !exchangeNode.IsAllow;
                }
                else
                {
                    exchange.IsExchangeAllow = false;
                    exchange.IsExchangeDeny = false;
                }
                RoleDataPermission accountPermission = dataPermissions.SingleOrDefault(d => d.ExchangeCode == item && d.Code.ToLower() == "account");
                if (accountPermission != null)
                {
                    account.DataObjectTypeId = accountPermission.PermissionId;
                    account.IsDataObjectTypeAllow = accountPermission.IsAllow;
                    account.IsDataObjectTypeDeny = !accountPermission.IsAllow;
                }
                else
                {
                    if (exchangeNode != null)
                    {
                        if (exchangeNode.IsAllow)
                        {
                            account.IsDataObjectTypeAllow = null;
                            account.IsDataObjectTypeDeny = false;
                        }
                        else
                        {
                            account.IsDataObjectTypeAllow = false;
                            account.IsDataObjectTypeDeny = null;
                        }
                    }
                    else
                    {
                        account.IsDataObjectTypeAllow = false;
                        account.IsDataObjectTypeDeny = false;
                    }
                }
                RoleDataPermission instrumentPermission = dataPermissions.SingleOrDefault(d => d.ExchangeCode == item && d.Code.ToLower() == "instrument");
                if (instrumentPermission != null)
                {
                    instrument.DataObjectTypeId = instrumentPermission.PermissionId;
                    instrument.IsDataObjectTypeAllow = instrumentPermission.IsAllow;
                    instrument.IsDataObjectTypeDeny = !instrumentPermission.IsAllow;
                }
                else
                {
                    if (exchangeNode != null)
                    {
                        if (exchangeNode.IsAllow)
                        {
                            instrument.IsDataObjectTypeAllow = null;
                            instrument.IsDataObjectTypeDeny = false;
                        }
                        else
                        {
                            instrument.IsDataObjectTypeAllow = false;
                            instrument.IsDataObjectTypeDeny = null;
                        }
                    }
                    else
                    {
                        instrument.IsDataObjectTypeAllow = false;
                        instrument.IsDataObjectTypeDeny = false;
                    }
                }
                List<RoleDataPermission> dataObjects = allDatas.FindAll(delegate(RoleDataPermission permission)
                {
                    return permission.ExchangeCode == item;
                });
                foreach (RoleDataPermission dataObject in dataObjects)
                {
                    DataObjectGridData Object = new DataObjectGridData();
                    Object.DataObjectId = dataObject.DataObjectId;
                    Object.Code = dataObject.Code;
                    RoleDataPermission dataObjectPermission = dataPermissions.SingleOrDefault(d => d.DataObjectId == dataObject.DataObjectId);
                    if (dataObjectPermission != null)
                    {
                        Object.Id = dataObjectPermission.PermissionId;
                        Object.IsAllow = dataObjectPermission.IsAllow;
                        Object.IsDeny = !dataObjectPermission.IsAllow;
                    }
                    else
                    {
                        if (dataObject.Type == DataObjectType.Account)
                        {
                            if (accountPermission != null)
                            {
                                if (accountPermission.IsAllow)
                                {
                                    Object.IsAllow = null;
                                    Object.IsDeny = false;
                                }
                                else
                                {
                                    Object.IsAllow = false;
                                    Object.IsDeny = null;
                                }
                            }
                            else
                            {
                                if (exchangeNode != null)
                                {
                                    if (exchangeNode.IsAllow)
                                    {
                                        Object.IsAllow = null;
                                        Object.IsDeny = false;
                                    }
                                    else
                                    {
                                        Object.IsAllow = false;
                                        Object.IsDeny = null;
                                    }
                                }
                                else
                                {
                                    Object.IsAllow = false;
                                    Object.IsDeny = false;
                                }
                            }
                        }
                        else if (dataObject.Type == DataObjectType.Instrument)
                        {
                            if (instrumentPermission != null)
                            {
                                if (instrumentPermission.IsAllow)
                                {
                                    Object.IsAllow = null;
                                    Object.IsDeny = false;
                                }
                                else
                                {
                                    Object.IsAllow = false;
                                    Object.IsDeny = null;
                                }
                            }
                            else
                            {
                                if (exchangeNode != null)
                                {
                                    if (exchangeNode.IsAllow)
                                    {
                                        Object.IsAllow = null;
                                        Object.IsDeny = false;
                                    }
                                    else
                                    {
                                        Object.IsAllow = false;
                                        Object.IsDeny = null;
                                    }
                                }
                                else
                                {
                                    Object.IsAllow = false;
                                    Object.IsDeny = false;
                                }
                            }
                        }
                    }
                    if (dataObject.Type == DataObjectType.Account)
                    {
                        account.DataObjects.Add(Object);
                    }
                    else if (dataObject.Type == DataObjectType.Instrument)
                    {
                        instrument.DataObjects.Add(Object);
                    }
                }
                exchange.DataObjectTypes.Add(account);
                exchange.DataObjectTypes.Add(instrument);
                data.Add(exchange);
            }
            IExchangeCodes = data;
        }

        public List<RoleDataPermission> CastGridDataToDataPermission()
        {
            List<RoleDataPermission> roleDataPermissions = new List<RoleDataPermission>();
            foreach (ExchangeGridData exchange in IExchangeCodes)
            {
                if (exchange.IsExchangeAllow == true || exchange.IsExchangeDeny == true)
                {
                    RoleDataPermission data = new RoleDataPermission();
                    data.PermissionId = exchange.ExchangeId;
                    data.Code = exchange.ExchangeCode;
                    data.ParentId = 2;
                    data.Level = 1;
                    data.ExchangeCode = exchange.ExchangeCode;
                    data.Type = DataObjectType.None;
                    if (exchange.IsExchangeAllow == true)
                    {
                        data.IsAllow = true;
                    }
                    if (exchange.IsExchangeDeny == true)
                    {
                        data.IsAllow = false;
                    }
                    roleDataPermissions.Add(data);
                }
                foreach (DataObjectTypeGridData type in exchange.DataObjectTypes)
                {
                    if (type.IsDataObjectTypeAllow == true || type.IsDataObjectTypeDeny == true)
                    {
                        RoleDataPermission data = new RoleDataPermission();
                        data.PermissionId = type.DataObjectTypeId;
                        data.Code = type.DataObjectType;
                        data.Type = (DataObjectType)Enum.Parse(typeof(DataObjectType), type.DataObjectType);
                        data.ParentId = exchange.ExchangeId;
                        data.Level = 2;
                        data.ExchangeCode = exchange.ExchangeCode;
                        if (type.IsDataObjectTypeAllow == true)
                        {
                            data.IsAllow = true;
                        }
                        if (type.IsDataObjectTypeDeny == true)
                        {
                            data.IsAllow = false;
                        }
                        roleDataPermissions.Add(data);
                    }
                    foreach (DataObjectGridData dataObject in type.DataObjects)
                    {
                        if (dataObject.IsAllow == true || dataObject.IsDeny == true)
                        {
                            RoleDataPermission data = new RoleDataPermission();
                            data.PermissionId = dataObject.Id;
                            data.Code = dataObject.Code;
                            data.DataObjectId = dataObject.DataObjectId;
                            data.ParentId = type.DataObjectTypeId;
                            data.Type = (DataObjectType)Enum.Parse(typeof(DataObjectType), type.DataObjectType);
                            data.Level = 3;
                            data.ExchangeCode = exchange.ExchangeCode;
                            if (dataObject.IsAllow == true)
                            {
                                data.IsAllow = true;
                            }
                            if (dataObject.IsDeny == true)
                            {
                                data.IsAllow = false;
                            }
                            roleDataPermissions.Add(data);
                        }
                    }
                }
            }
            return roleDataPermissions;
        }
    }

    public class ExchangeGridData : INotifyPropertyChanged
    {
        #region private
        private bool _isExchangeAllow = false;
        private bool _isExchangeDeny = false;
        #endregion

        #region public
        public int ExchangeId { get; set; }
        public string ExchangeCode { get; set; }
        public bool IsExchangeAllow
        {
            get
            {
                return _isExchangeAllow;
            }
            set
            {
                if (value == true)
                {
                    _isExchangeAllow = value;
                    _isExchangeDeny = !value;
                    this.SetChildDeafult(null, false);
                }
                else
                {
                    _isExchangeAllow = value;
                }
                
                if (_isExchangeDeny != true)
                {
                    this.SetChildIsAllow(value);
                }
                this.NotifyPropertyChanged("IsIExchangeAllow");
                this.NotifyPropertyChanged("IsIExchangeDeny");
            }
        }
        public bool IsExchangeDeny
        {
            get
            {
                return _isExchangeDeny;
            }
            set
            {
                if (value == true)
                {
                    _isExchangeDeny = value;
                    _isExchangeAllow = !value;
                    this.SetChildDeafult(false, null);
                }
                else
                {
                    _isExchangeDeny = value;
                }
                if (_isExchangeAllow != true)
                {
                    this.SetChildIsDeny(value);
                }
                this.NotifyPropertyChanged("IsIExchangeAllow");
                this.NotifyPropertyChanged("IsIExchangeDeny");
            }
        }
        public ObservableCollection<DataObjectTypeGridData> DataObjectTypes { get; set; }
        #endregion

        #region Constructor
        public ExchangeGridData()
        {
            DataObjectTypes = new ObservableCollection<DataObjectTypeGridData>();
        }
        #endregion

        #region event
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region method
        private void SetChildIsAllow(bool? isAllow)
        {
            if (isAllow == false)
            {
                foreach (DataObjectTypeGridData type in DataObjectTypes)
                {
                    if (type.IsDataObjectTypeAllow == null || type.IsDataObjectTypeDeny == null || type.IsDataObjectTypeDeny == type.IsDataObjectTypeAllow)
                    {
                        type.SetDeafult(false, false);
                        type.SetPermission(false, false);
                    }
                }
            }
            else
            {
                foreach (DataObjectTypeGridData type in DataObjectTypes)
                {
                    if (type.IsDataObjectTypeAllow == null || type.IsDataObjectTypeDeny == null || type.IsDataObjectTypeDeny == type.IsDataObjectTypeAllow)
                    {
                        type.SetDeafult(null, false);
                        type.SetPermission(null, false);
                    }
                }
            }
        }

        private void SetChildIsDeny(bool? isDeny)
        {
            if (isDeny == false)
            {
                foreach (DataObjectTypeGridData type in DataObjectTypes)
                {
                    if (type.IsDataObjectTypeAllow == null || type.IsDataObjectTypeDeny == null || type.IsDataObjectTypeDeny == type.IsDataObjectTypeAllow)
                    {
                        type.SetDeafult(false, false);
                        type.SetPermission(_isExchangeAllow, false);
                    }
                }
            }
            else
            {
                foreach (DataObjectTypeGridData type in DataObjectTypes)
                {
                    if (type.IsDataObjectTypeAllow == null || type.IsDataObjectTypeDeny == null || type.IsDataObjectTypeDeny == type.IsDataObjectTypeAllow)
                    {
                        type.SetDeafult(false, null);
                        type.SetPermission(false, null);
                    }
                }
            }
        }
        private void SetChildDeafult(bool? isAllow, bool? isDeny)
        {
            foreach (DataObjectTypeGridData type in DataObjectTypes)
            {
                type.SetDeafult(isAllow, isDeny);
            }
        }

        public bool CheckDataIsValid()
        {
            bool isValid = true;
            if (IsExchangeAllow == false && IsExchangeDeny == false)
            {
                foreach (DataObjectTypeGridData type in DataObjectTypes)
                {
                    if (type.IsDataObjectTypeAllow == false && type.IsDataObjectTypeDeny == false)
                    {
                        foreach (DataObjectGridData data in type.DataObjects)
                        {
                            if (data.IsAllow == false && data.IsDeny == false)
                            {
                                isValid = false;
                            }
                        }
                    }
                }
            }
            return isValid;
        }
        #endregion
    }

    public class DataObjectTypeGridData : INotifyPropertyChanged
    {
        #region private
        private bool? _isDataObjectTypeAllow = false;
        private bool? _isDataObjectTypeDeny = false;
        private bool? _deafultTypeAllow = false;
        private bool? _deafultTypeDeny = false;
        #endregion

        #region public
        public int DataObjectTypeId { get; set; }
        public string DataObjectType { get; set; }
        public bool? IsDataObjectTypeAllow
        {
            get
            {
                return _isDataObjectTypeAllow;
            }
            set
            {
                if (_isDataObjectTypeAllow != null)
                {
                    if (value == true)
                    {
                        _isDataObjectTypeAllow = value;
                        _isDataObjectTypeDeny = !value;
                        this.SetChildDeafult(null, false);
                    }
                    else
                    {
                        this.SetChildDeafult(_deafultTypeAllow, _deafultTypeDeny);
                        _isDataObjectTypeAllow = value;
                    }
                    if (_isDataObjectTypeDeny == false)
                    {
                        this.SetChildIsAllow(value);
                        if (_isDataObjectTypeAllow == false)
                        {
                            _isDataObjectTypeDeny = _deafultTypeDeny;
                        }
                    }
                    this.NotifyPropertyChanged("IsDataObjectTypeAllow");
                    this.NotifyPropertyChanged("IsDataObjectTypeDeny");
                }
            }
        }
        public bool? IsDataObjectTypeDeny
        {
            get
            {
                return _isDataObjectTypeDeny;
            }
            set
            {
                if (_isDataObjectTypeDeny != null)
                {
                    if (value == true)
                    {
                        _isDataObjectTypeDeny = value;
                        _isDataObjectTypeAllow = !value;
                        this.SetChildDeafult(false, null);
                    }
                    else
                    {
                        this.SetChildDeafult(_deafultTypeAllow, _deafultTypeDeny);
                        _isDataObjectTypeDeny = value;
                    }
                    if (_isDataObjectTypeAllow == false)
                    {
                        this.SetChildIsDeny(value);
                        if (_isDataObjectTypeDeny == false)
                        {
                            _isDataObjectTypeAllow = _deafultTypeAllow;
                        }
                    }
                    this.NotifyPropertyChanged("IsDataObjectTypeAllow");
                    this.NotifyPropertyChanged("IsDataObjectTypeDeny");
                }
            }
        }
        public ObservableCollection<DataObjectGridData> DataObjects { get; set; }
        #endregion

        #region Constructor
        public DataObjectTypeGridData()
        {
            DataObjects = new ObservableCollection<DataObjectGridData>();
        }
        #endregion

        #region event
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region method
        private void SetChildIsAllow(bool? isAllow)
        {
            if (isAllow == false)
            {
                foreach (DataObjectGridData dataGrid in DataObjects)
                {
                    if (dataGrid.IsAllow == null || dataGrid.IsDeny == null || dataGrid.IsAllow == dataGrid.IsDeny)
                    {
                        dataGrid.SetPermission(false, _deafultTypeDeny);
                        dataGrid.ResetDeafult(false, _deafultTypeDeny);
                    }
                }
            }
            else
            {
                foreach (DataObjectGridData dataGrid in DataObjects)
                {
                    if (dataGrid.IsAllow == null || dataGrid.IsDeny == null || dataGrid.IsAllow == dataGrid.IsDeny)
                    {
                        dataGrid.SetPermission(null, false);
                        dataGrid.ResetDeafult(null, false);
                    }
                }
            }
        }

        private void SetChildIsDeny(bool? isDeny)
        {
            if (isDeny == false)
            {
                foreach (DataObjectGridData dataGrid in DataObjects)
                {
                    if (dataGrid.IsAllow == null || dataGrid.IsDeny == null || dataGrid.IsAllow == dataGrid.IsDeny)
                    {
                        dataGrid.SetPermission(_deafultTypeAllow, false);
                        dataGrid.ResetDeafult(_deafultTypeAllow, false);
                        
                    }
                }
            }
            else
            {
                foreach (DataObjectGridData dataGrid in DataObjects)
                {
                    if (dataGrid.IsAllow == null || dataGrid.IsDeny == null || dataGrid.IsAllow == dataGrid.IsDeny)
                    {
                        dataGrid.SetPermission(false, null);
                        dataGrid.ResetDeafult(false, null);
                    }
                }
            }
        }

        public void SetPermission(bool? isDataObjectTypeAllow, bool? isDataObjectTypeDeny)
        {
            this._isDataObjectTypeAllow = isDataObjectTypeAllow;
            this._isDataObjectTypeDeny = isDataObjectTypeDeny;
            if (_isDataObjectTypeAllow != isDataObjectTypeAllow)
            {
                this.SetChildIsAllow(isDataObjectTypeAllow);
            }
            if (_isDataObjectTypeDeny == isDataObjectTypeDeny)
            {
                this.SetChildIsDeny(isDataObjectTypeDeny);
            }
            this.NotifyPropertyChanged("IsDataObjectTypeAllow");
            this.NotifyPropertyChanged("IsDataObjectTypeDeny");
        }

        public void SetDeafult(bool? isAllow, bool? isDeny)
        {
            this._deafultTypeAllow = isAllow;
            this._deafultTypeDeny = isDeny;
        }

        public void SetChildDeafult(bool? isAllow, bool? isDeny)
        {
            foreach (DataObjectGridData dataGrid in DataObjects)
            {
                dataGrid.ResetDeafult(isAllow, isDeny);
            }
        }
        #endregion
    }

    public class DataObjectGridData : INotifyPropertyChanged
    {
        #region private
        private bool? _isAllow = false;
        private bool? _isDeny = false;
        private bool? _deafultAllow = false;
        private bool? _deafultDeny = false;
        #endregion

        #region public
        public int Id { get; set; }
        public Guid DataObjectId { get; set; }
        public string Code { get; set; }
        public bool? IsAllow
        {
            get
            {
                return _isAllow;
            }
            set
            {
                if (_isAllow != null)
                {
                    if (value == true)
                    {
                        if (_deafultAllow == null)
                        {
                            _isAllow = null;
                        }
                        else
                        {
                            _isAllow = value;
                        }
                        if (_isDeny == null)
                        {
                            _deafultDeny = null;
                        }
                        _isDeny = !value;

                    }
                    else
                    {
                        if (value == false && _deafultDeny == null)
                        {
                            _isDeny = _deafultDeny;
                        }
                        _isAllow = value;
                    }
                    this.NotifyPropertyChanged("IsAllow");
                    this.NotifyPropertyChanged("IsDeny");
                }
            }
        }
        public bool? IsDeny
        {
            get
            {
                return _isDeny;
            }
            set
            {
                if (_isDeny != null)
                {
                    if (value == true)
                    {
                        if (_deafultDeny == null)
                        {
                            _isDeny = null;
                        }
                        else
                        {
                            _isDeny = value;
                        }
                        if (_isAllow == null)
                        {
                            _deafultAllow = null;
                        }
                        _isAllow = !value;
                    }
                    else
                    {
                        if (value == false && _deafultAllow == null)
                        {
                            _isAllow = _deafultAllow;
                        }
                        _isDeny = value;
                    }
                    this.NotifyPropertyChanged("IsAllow");
                    this.NotifyPropertyChanged("IsDeny");
                }
            }
        }
        #endregion

        #region method
        public void SetPermission(bool? isAllow, bool? isDeny)
        {
            this._isAllow = isAllow;
            this._isDeny = isDeny;
            this.NotifyPropertyChanged("IsAllow");
            this.NotifyPropertyChanged("IsDeny");
        }

        public void ResetDeafult(bool? isAllow, bool? isDeny)
        {
            this._deafultAllow = isAllow;
            this._deafultDeny = isDeny;
        }
        #endregion

        #region event
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}

    #endregion