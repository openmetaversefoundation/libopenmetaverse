////////////////////////////////////////////////////////////////////////////////
// World Server
////////////////////////////////////////////////////////////////////////////////

&region_info =
{
	name: string,
	id: uuid,
	handle: binary,
	online: boolean,
	ip: binary,
	port: integer,
	address: uri,
	map_texture_id: uuid,
	owner: uri,
	region_flags: integer,
	agent_count: integer,
	capabilities: { $: uri },
	water_height: real,
	default_position: [ real, real, real ],
	default_look_at: [ real, real, real ]
}

%%create_region

-> region_info

<-
{
	success: boolean,
	message: string,
	simulation_key: uuid
}

%%delete_region

->
{
	id: uuid,
	simulation_key: uuid
}

<-
{
	success: boolean,
	message: string
}

%%region_update

->
{
	region: region_info,
	simulation_key: uuid
}

<-
{
	success: boolean,
	message: string
}

%%fetch_region

&fetch_region_query = { id: uuid }
&fetch_region_query = { x: integer, y: integer, nearest: boolean }
&fetch_region_query = { name: string }

-> fetch_region_query

<- region_info

%%fetch_default_region

<- region_info

%%region_search

->
{
	query: string
}

<-
{
	regions: [ region_info, ... ]
}

%%get_region_count

<-
{
	count: integer
}

////////////////////////////////////////////////////////////////////////////////
// Identity
////////////////////////////////////////////////////////////////////////////////

// This message should be ignored if it originates from anywhere other than the
// identity server for the given identity

%%request_capabilities

->
{
    identity: uri,
    capabilities: [ string, ... ]
}

<-
{
    capabilities: { $: uri }
}

////////////////////////////////////////////////////////////////////////////////
// Assets
////////////////////////////////////////////////////////////////////////////////

%%get_asset_metadata

&metadata = 
{
    id: uuid,
    name: string,
    description: string,
    creation_date: date,
    content_type: string,
    sha256: binary,
    temporary: boolean,
    methods: { $: uri }
}

&metadata = 
{
    id: uuid,
    name: string,
    description: string,
    creation_date: date,
    content_type: string,
    sha256: binary,
    temporary: boolean,
    methods: { $: uri }
    components: integer,
    layer_ends: [ integer ]
}

<- metadata

%%create_asset

&create =
{
    content_type: string,
    temporary: boolean,
    base64_data: string
}

&create =
{
    id: uuid,
    content_type: string,
    temporary: boolean,
    base64_data: string
}

-> create

<-
{
    id: uuid
}

////////////////////////////////////////////////////////////////////////////////
// Inventory
////////////////////////////////////////////////////////////////////////////////

%%create_inventory

->
{
    identity: uri,
    name: string
}

<-
{
    root_folder_id: uuid
}

%%create_folder

->
{
    identity: uri,
    folder_id: uuid,
    parent_id: uuid,
    name: string,
    preferred_content_type: string
}

%%purge_folder

->
{
    identity: uri,
    folder_id: uuid
}

%%create_item

->
{
    identity: uri,
    name: string,
    description: string,
    content_type: string,
    asset_id: uuid,
    parent_id: uuid
}

<-
{
    item_id: uuid
}

%%move_item

->
{
    identity: uri,
    item_id: uuid,
    new_parent_id: uuid,
    new_name: string
}

<-
{
    item_id: uuid
}

%%update_item

->
{
    identity: uri,
    name: string,
    description: string,
    content_type: string,
    asset_id: uuid
}

%%get_inventory_skeleton

->
{
    identity: uri
}

<-
{
    folders:
    [
        {
            name: string,
            parent_id: uuid,
            version: integer,
            preferred_content_type: string,
            folder_id: uuid
        },
        ...
    ]
}

%%get_active_gestures

->
{
    identity: uri
}

<-
{
    gestures:
    [
        {
            item_id: uuid,
            asset_id: uuid
        },
        ...
    ]
}

////////////////////////////////////////////////////////////////////////////////
// Simulators
////////////////////////////////////////////////////////////////////////////////

%%enable_client

->
{
    agent_id: uuid,
    session_id: uuid,
    secure_session_id: uuid,
    circuit_code: integer,
    region_handle: binary,
    child_agent: boolean,
    ip: binary,
    client_version: string,
    attributes: { },
    services: { $: { $: uri } },
    callback_uri: uri
}

<-
{
    success: boolean,
    message: string,
    seed_capability: uri,
}

%%enable_client_complete

->
{
    agent_id: uuid
}

%%close_agent_connection

->
{
    agent_id: uuid
}

%%neighbor_update

->
{
	neighbors: [ region_info, ... ]
}

%%child_agent_update

->
{
    agent_id: uuid,
    session_id: uuid,
    position: [ real, real, real ],
    velocity: [ real, real, real ],
    region_handle: binary,
    cam_position: [ real, real, real ],
    cam_at_axis: [ real, real, real ],
    cam_left_axis: [ real, real, real ],
    cam_up_axis: [ real, real, real ]
}

%%pass_object

->
{
    id: uuid
}

<-
{
    success: boolean,
    message: string
}

%%fetch_terrain

->
{
    blocks:
    [
        {
            x: integer,
            y: integer
        }
    ]
}

<-
{
    blocks:
    [
        {
            x: integer,
            y: integer,
            data: binary
        }
    ]
}
